using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Services.Logs;
using System.Collections.Concurrent;
using System.Text;
using System.Xml.Linq;
namespace NanTingBlog.API.Services.Blog;

/// <summary>
/// 监听文章变化，以更新数据库
/// </summary>
public class WatchService : BackgroundService
{
    private readonly IServiceScopeFactory factory;
    private readonly GlobalConfigService gconfig;
    private readonly FileSystemWatcher fileWatcher;
    private readonly BlockingCollection<Func<PostsService, Task>> uploadings;
    private readonly ConcurrentDictionary<string, string> uidToName;
    private readonly ConcurrentDictionary<string, string> nameToUid;
    private readonly ServiceLogger logger;

    /// <summary></summary>
    public WatchService(
        IServiceScopeFactory serviceFactory,
        GlobalConfigService gconfig,
        ServiceLogger logger)
    {
        this.logger = logger;
        factory = serviceFactory;
        this.gconfig = gconfig;
        if (!Directory.Exists(gconfig.BlogSaveDir)) {
            Directory.CreateDirectory(gconfig.BlogSaveDir!);
        }
        fileWatcher = new FileSystemWatcher(gconfig.BlogSaveDir!, "*.md")
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        uploadings = [];
        uidToName = [];
        nameToUid = [];

        fileWatcher.Changed += FileChanged;
        fileWatcher.Created += FileCreated;
        fileWatcher.Deleted += FileDeleted;
        fileWatcher.Renamed += FileRenamed;
    }

    private void FileRenamed(object sender, RenamedEventArgs e)
    {
        if(e.OldName == null || e.Name == null) return;
        // 先从旧名称获取uid
        if (!TryGetUid(Path.GetFileNameWithoutExtension(e.OldName) ?? "", out string uid)) {
            return;
        }

        // 更新文章名称
        uploadings.Add(async service => {
            var postName = Path.GetFileNameWithoutExtension(e.Name);
            UpdateName(uid, postName);
            var blogInfo = await service.QueryByKeyAsync(uid);
            if(blogInfo == null) return;
            logger.Information($"更新本地文章文件名称 `{blogInfo.Name}` -> `{postName}` ");
            blogInfo.Name = postName; // 前面用的是e.name 导致保存了扩展名
            await service.UpdateOrAddAsync(blogInfo);
        });
    }

    private void FileDeleted(object sender, FileSystemEventArgs e)
    {
        uploadings.Add( async service => {
            var fileName = Path.GetFileNameWithoutExtension(e.FullPath);
            if (TryGetUid(fileName, out string id)) {
                await service.DeleteByKeyAsync(id); // 删除文章
            }
            logger.Information($"删除了本地文章文件 `{fileName}`");
            RemoveName(fileName);
        });
    }

    private void FileCreated(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(e.FullPath)) {
            return;
        }

        uploadings.Add(async service => {
            var fileName = Path.GetFileNameWithoutExtension(e.FullPath);
            var blogText = File.ReadAllText(e.FullPath);
            var bi = new PostInfo()
            {
                CreateTime = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks,
                EditTime = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks,
                Name = fileName,
                Content = blogText
            };
            await service.UpdateOrAddAsync(bi);
            File.WriteAllText(e.FullPath, bi.Content);
            AddName(fileName, bi.Id);
            logger.Information($"本地创建了一个文章文件 `{fileName}`");
        });
    }

    private void FileChanged(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(e.FullPath)) {
            return;
        }

        uploadings.Add(async service => {
            var blogName = Path.GetFileNameWithoutExtension(e.FullPath);
            if (!TryGetUid(blogName, out var id)) {
                return;
            }
            var blogText = File.ReadAllText(e.FullPath);
            var blog = await service.QueryByKeyAsync(id);
            if(blog == null) return;
            blog.Content = blogText;
            blog.EditTime = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks;
            logger.Information($"本地文章内容发生了改变 `{blogName}`");
            await service.UpdateOrAddAsync(blog);
        });
    }

    /// <summary> </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(10, stoppingToken);
        fileWatcher.EnableRaisingEvents = false;
        await InitMap();
        fileWatcher.EnableRaisingEvents = true;
        while (!stoppingToken.IsCancellationRequested) {
            if (uploadings.TryTake(out var result, Timeout.Infinite, stoppingToken)) {
                using (var serviceScope = factory.CreateScope()) {
                    var postsService = serviceScope.ServiceProvider.GetService<PostsService>();
                    try {
                        await result(postsService!);
                    } catch{}
                }
            }
        }
    }

    /// <summary>
    /// 创建新文章到新文件
    /// </summary>
    public async Task Create(PostInfo info)
    {
        uploadings.Add(async _ => {
            logger.Information($"创建新文章 `{info.Name}` ");
            var fullPath = Path.Combine(gconfig.BlogSaveDir!, "api_create", info.Name + ".md");
            var stream = File.CreateText(fullPath);
            await stream.WriteAsync(info.Content);
            await stream.FlushAsync();
            stream.Dispose();
        });
        
    }

    /// <summary>
    /// 重命名文章
    /// </summary>
    public void Rename(string oldName, string newName)
    {
        if (!TryGetMarkdownFullName(oldName, out var postFullPath)) {
            return;
        }
        uploadings.Add(_ => {
            logger.Information($"更新文章文件名称 `{oldName}` -> `{newName}` ");
            var targetPath = Path.Combine(Path.GetDirectoryName(postFullPath)!, newName + ".md");
            File.Move(postFullPath, targetPath, true);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// 更新文章内容
    /// </summary>
    public void Update(string name, string newContent)
    {
        uploadings.Add(_ => {
            if (!TryGetMarkdownFullName(name, out var fullPath)) {
                return Task.CompletedTask;
            }
            logger.Information($"更新文章内容 `{name}`");
            var stream = File.Create(fullPath);
            stream.Write(Encoding.UTF8.GetBytes(newContent));
            stream.Dispose();
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// 删除本地文件，不会通知数据库
    /// </summary>
    public void Delete(string key)
    {
        if (!TryGetName(key, out var name)) {
            return;
        }
        if (!TryGetMarkdownFullName(name, out var fullPath)) {
            return;
        }
        logger.Information($"删除本地内容: `{fullPath}`");
        try {
            logger.Information(File.ReadAllText(fullPath));
            File.Delete(fullPath);
        } catch {
        }
    }

    private void UpdateName(string uid, string newName)
    {
        if (!uidToName.TryGetValue(uid, out var oldName)) {
            return;
        }
        nameToUid.Remove(oldName, out _);
        nameToUid.AddOrUpdate(newName, uid, (old, @new) => @new);
        uidToName.AddOrUpdate(uid, newName, (old, @new) => @new);
    }

    private void RemoveName(string name)
    {
        if(TryGetUid(name, out var id)) {
            _ = uidToName.TryRemove(id, out _);
        }
        _ = nameToUid.TryRemove(name, out _);
    }

    private void AddName(string name, string id)
    {
        nameToUid.AddOrUpdate(name, id, (old, @new) => @new);
        uidToName.AddOrUpdate(id, name, (old, @new) => @new);
    }

    private bool TryGetUid(string? name, out string value)
    {
        value = string.Empty;
        if(name == null) return false;
        return nameToUid.TryGetValue(name, out value!);
    }

    private bool TryGetName(string? uid, out string value)
    {
        value = string.Empty;
        if (uid == null) return false;
        return uidToName.TryGetValue(uid, out value!);
    }

    /// <summary>
    /// 初始化<see cref="nameToUid"/> 和 <see cref="uidToName"/>
    /// </summary>
    /// <returns></returns>
    private async Task InitMap()
    {
        logger.Information($"初始化文章监听服务");
        using var serviceScope = factory.CreateScope();
        var postsService = serviceScope.ServiceProvider.GetService<PostsService>();

        #region 从数据库同步
        foreach (var postInfo in postsService!.QueryAll()) {
            uidToName[postInfo.Id] = postInfo.Name;
            nameToUid[postInfo.Name] = postInfo.Id;
        }
        #endregion

        if (!Directory.Exists(gconfig.BlogSaveDir)) {
            Directory.CreateDirectory(gconfig.BlogSaveDir!);
            logger.Information($"文章文件夹不存在，进行创建 {gconfig.BlogSaveDir}");
        }

        #region 获取本地的内容
        var files = RecursivelyGetFullFile(gconfig.BlogSaveDir!);
        var localhostBlogs = files
            .Where(fullPath => Path.GetExtension(fullPath) == ".md")
            .Select(fullPath => (fullPath: fullPath, blogName: Path.GetFileNameWithoutExtension(fullPath)))
            .ToList();

        logger.Information($"本地文章数量: {localhostBlogs.Count}");
        #endregion

        #region 从本地同步到数据库
        foreach ((string fullPath, string blogName) in localhostBlogs) {
            if(TryGetUid(blogName, out _)) { // 存在 跳过
                continue;
            }
            logger.Information($"将 `{blogName}` 同步到数据库");
            var blogText = File.ReadAllText(fullPath);
            var fileInfo = new FileInfo(fullPath);
            var newBlog = new PostInfo()
            {
                Name = blogName,
                Content = blogText,
                CreateTime = fileInfo.CreationTimeUtc.Ticks - DateTimeOffset.UnixEpoch.Ticks,
                EditTime = fileInfo.LastWriteTimeUtc.Ticks - DateTimeOffset.UnixEpoch.Ticks
            };
            await postsService.UpdateOrAddAsync(newBlog);
        }
        #endregion

        #region 迁移YAML头
        logger.Information($"迁移全部文章的yaml头信息 - 开始");
        await postsService.MigrateAllYamlHeadersAsync();
        logger.Information($"迁移全部文章的yaml头信息 - 结束");
        #endregion

        #region 从数据库全量覆盖到本地
        logger.Information("从数据库将内容全量覆盖到本地 - 开始");
        // 重新加载数据库数据到映射（包含新上传的文章）
        uidToName.Clear();
        nameToUid.Clear();

        var syncByDbSavePath = Path.Combine(gconfig.BlogSaveDir!, "db_sync");
        if(!Directory.Exists(syncByDbSavePath)) {
            Directory.CreateDirectory(syncByDbSavePath);
        }

        DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        foreach (var postInfo in postsService.QueryAll()) {
            // 更新映射
            uidToName[postInfo.Id] = postInfo.Name;
            nameToUid[postInfo.Name] = postInfo.Id;

            // 优先查找本地已有路径，保持原路径不变
            if (TryGetMarkdownFullName(postInfo.Name, out var existingFullPath)) {
                using (var stream = File.CreateText(existingFullPath)) {
                    await stream.WriteAsync(postInfo.Content);
                    await stream.FlushAsync();
                }
            } else {
                // 本地不存在则创建到 db_sync 目录
                var saveFullPath = Path.Combine(syncByDbSavePath, postInfo.Name + ".md");
                using (var stream = File.CreateText(saveFullPath)) {
                    await stream.WriteAsync(postInfo.Content);
                    await stream.FlushAsync();
                }
                var fileInfo = new FileInfo(saveFullPath)
                {
                    CreationTime = baseTime.Add(TimeSpan.FromTicks(postInfo.CreateTime))
                };
            }
        }
        logger.Information("从数据库将内容全量覆盖到本地 - 结束");
        #endregion

    }

    private static IEnumerable<string> RecursivelyGetFullFile(string rootDirPath)
    {
        if (!Directory.Exists(rootDirPath)) {
            yield break;
        }
        foreach (var file in Directory.GetFiles(rootDirPath)) {
            yield return file;
        }
        var dirs = Directory.GetDirectories(rootDirPath);
        foreach (var dir in dirs) {
            foreach (var file in RecursivelyGetFullFile(dir)) {
                yield return file;
            };
        }
    }

    /// <summary>
    /// 获取此目录下的全部md文件
    /// </summary>
    private static IEnumerable<string> RecursivelyGetFullFileByMarkdonw(string rootDirPath)
    {
        foreach (var item in RecursivelyGetFullFile(rootDirPath)) {
            if (Path.GetExtension(item) == ".md") {
                yield return item;
            }
        }
    }

    private bool TryGetMarkdownFullName(string notExtensionName, out string reslut)
    {
        reslut = "";
        foreach (var mdFullPath in RecursivelyGetFullFileByMarkdonw(gconfig.BlogSaveDir!)) {
            if (!(Path.GetFileNameWithoutExtension(mdFullPath) == notExtensionName)) {
                continue;
            }
            reslut = mdFullPath;
            return true;
        }
        return false;
    }
}