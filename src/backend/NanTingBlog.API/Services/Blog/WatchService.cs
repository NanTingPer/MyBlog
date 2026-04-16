using NanTingBlog.API.Dtos.Blogs;
using System.Collections.Concurrent;
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
    private readonly BlockingCollection<Func<InterviewService, Task>> uploadings;
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
            UpdateName(uid, Path.GetFileNameWithoutExtension(e.Name));
            var blogInfo = await service.SearchOnId(uid);
            if(blogInfo == null) return;
            blogInfo.Name = e.Name;
            await service.AddOrReplace(blogInfo);
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
                await service.Delete(id); // 删除文章
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
            var bi = new BlogInfo()
            {
                CreateTime = DateTime.UtcNow.Ticks,
                EditTime = DateTime.UtcNow.Ticks,
                Name = fileName
            };
            await service.AddOrReplace(bi);
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
            var blog = await service.SearchOnId(id);
            if(blog == null) return;
            blog.Content = blogText;
            blog.EditTime = DateTime.UtcNow.Ticks;
            await service.AddOrReplace(blog);
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
                    var interviewService = serviceScope.ServiceProvider.GetService<InterviewService>();
                    try {
                        await result(interviewService!);
                    } catch{}
                }
            }
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
        using var serviceScope = factory.CreateScope();
        var interviewService = serviceScope.ServiceProvider.GetService<InterviewService>();
        int page = 1;
        IReadOnlyCollection<BlogInfo> infos;
        while((infos = await interviewService!.Search(new Meilisearch.SearchQuery(){ Page = page})).Count != 0) {
            page += 1;
            foreach (var blogInfo in infos) {
                uidToName[blogInfo.Id] = blogInfo.Name;
                nameToUid[blogInfo.Name] = blogInfo.Id;
            }
        }

        // 上面是从数据库同步数据
        if (!Directory.Exists(gconfig.BlogSaveDir)) {
            Directory.CreateDirectory(gconfig.BlogSaveDir);
        }

        var files = RecursivelyGetFullFile(gconfig.BlogSaveDir);
        var blogs = files
            .Where(fullPath => Path.GetExtension(fullPath) == ".md")
            .Select(fullPath => (fullPath: fullPath, blogName: Path.GetFileNameWithoutExtension(fullPath)));

        foreach ((string fullPath, string blogName) in blogs) {
            if(TryGetUid(blogName, out _)) { // 存在 跳过
                continue;
            }
            var blogText = File.ReadAllText(fullPath);
            var fileInfo = new FileInfo(fullPath);
            var newBlog = new BlogInfo()
            {
                Name = blogName,
                Content = blogText,
                CreateTime = fileInfo.CreationTimeUtc.Ticks,
                EditTime = fileInfo.LastWriteTimeUtc.Ticks
            };
            await interviewService.AddOrReplace(newBlog);
        }
    }

    private static string[] RecursivelyGetFullFile(string rootDirPath)
    {
        if (!Directory.Exists(rootDirPath)) {
            return [];
        }
        var files = new HashSet<string>();
        foreach (var file in Directory.GetFiles(rootDirPath)) {
            files.Add(file);
        }
        var dirs = Directory.GetDirectories(rootDirPath);
        foreach (var dir in dirs) {
            foreach (var file in RecursivelyGetFullFile(dir)) {
                files.Add(file);
            };
        }
        return [.. files];
    }
}