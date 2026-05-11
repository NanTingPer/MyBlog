using NanTingBlog.API.Dtos.Blogs;
using System.Collections.Concurrent;
using System.Text;
using System.Xml.Linq;
namespace NanTingBlog.API.Services.Blog.Post;

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

    /// <summary></summary>
    public WatchService(
        IServiceScopeFactory serviceFactory,
        GlobalConfigService gconfig)
    {
        factory = serviceFactory;
        this.gconfig = gconfig;
        if (!Directory.Exists(gconfig.BlogSaveDir)) {
            Directory.CreateDirectory(gconfig.BlogSaveDir);
        }
        fileWatcher = new FileSystemWatcher(gconfig.BlogSaveDir, "*.md")
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
            var blogInfo = await service.QueryByKeyTrackingAsync(uid);
            if(blogInfo == null) return;
            blogInfo.Name = postName; // 前面用的是e.name 导致保存了扩展名
            await service.UpdateOrAddAsync(blogInfo);
        });
    }

    private void FileDeleted(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(e.FullPath)) {
            return; //文件夹
        }

        uploadings.Add( async service => {
            var fileName = Path.GetFileNameWithoutExtension(e.FullPath);
            RemoveName(fileName);
            if (TryGetUid(fileName, out string id)) {
                await service.DeleteByKeyAsync(id); // 删除文章
            }
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
            AddName(fileName, bi.Id);
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
            var blog = await service.QueryByKeyTrackingAsync(id);
            if(blog == null) return;
            blog.Content = blogText;
            blog.EditTime = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks;
            await service.UpdateOrAddAsync(blog);
        });
    }

    /// <summary> </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(10, stoppingToken);
        await InitMap();
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
            var fullPath = Path.Combine(gconfig.BlogSaveDir, "api_create", info.Name + ".md");
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
            var stream = File.Create(fullPath);
            stream.Write(Encoding.UTF8.GetBytes(newContent));
            stream.Dispose();
            return Task.CompletedTask;
        });
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
        using var serviceScope = factory.CreateScope();
        var postsService = serviceScope.ServiceProvider.GetService<PostsService>();
        #region 从数据库同步
        foreach (var postInfo in postsService!.QueryAllNoTracking()) {
            uidToName[postInfo.Id] = postInfo.Name;
            nameToUid[postInfo.Name] = postInfo.Id;
        }
        #endregion

        // 上面是从数据库同步数据
        if (!Directory.Exists(gconfig.BlogSaveDir)) {
            Directory.CreateDirectory(gconfig.BlogSaveDir);
        }

        #region 获取本地的内容
        var files = RecursivelyGetFullFile(gconfig.BlogSaveDir);
        var localhostBlogs = files
            .Where(fullPath => Path.GetExtension(fullPath) == ".md")
            .Select(fullPath => (fullPath: fullPath, blogName: Path.GetFileNameWithoutExtension(fullPath)));
        #endregion

        #region 从数据库同步到本地
        // nametouid => key是name => 检查本地博文中是否存在以注册的名称 => 如果any返回true说明有
        // 从数据库中有的，过滤出本地没有的
        var serverBlogs = nameToUid.Keys;
        var localhostNotExistBlog = serverBlogs.Where(f => !localhostBlogs.Any(fb => fb.blogName == f));
        var syncByDbSavePath = Path.Combine(gconfig.BlogSaveDir, "db_sync");
        DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        foreach (var blogName in localhostNotExistBlog) {
            if (!nameToUid.TryGetValue(blogName, out var key)) {
                continue;
            }
            var post = await postsService.QueryByKeyNoTrackingAsync(key);
            if (post == null) {
                continue;
            }
            var saveFullPath = Path.Combine(syncByDbSavePath, blogName + ".md");
            using (var stream = File.CreateText(saveFullPath)) {
                await stream.WriteAsync(post.Content);
                await stream.FlushAsync();
            }
            var fileInfo = new FileInfo(saveFullPath)
            {
                CreationTime = baseTime.Add(TimeSpan.FromTicks(post.CreateTime))
            };
        }
        #endregion

        #region 从本地同步到数据库
        foreach ((string fullPath, string blogName) in localhostBlogs) {
            if(TryGetUid(blogName, out _)) { // 存在 跳过
                continue;
            }
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
        foreach (var mdFullPath in RecursivelyGetFullFileByMarkdonw(gconfig.BlogSaveDir)) {
            if (!(Path.GetFileNameWithoutExtension(mdFullPath) == notExtensionName)) {
                continue;
            }
            reslut = mdFullPath;
            return true;
        }
        return false;
    }
}