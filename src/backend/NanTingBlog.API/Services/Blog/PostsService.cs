using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Extensions;
using NanTingBlog.API.ServiceModels;
using NanTingBlog.API.Services.Db;
using NanTingBlog.API.Utils;
using System.Linq.Expressions;

namespace NanTingBlog.API.Services.Blog;

/// <summary>
/// 文章服务
/// </summary>
public class PostsService(BlogContext context, IMemoryCache cache) : BaseRepository<PostInfo, string>(context)
{
    private readonly BlogContext context = context;
    private const string postsCacheKey = "AllPosts";

    /// <summary>
    /// 取key
    /// </summary>
    public override Expression<Func<PostInfo, string>> KeyExpression { get => field; init; } = f => f.Id;

    /// <summary>
    /// 查询全部
    /// </summary>
    public Task<List<PostInfo>> QueryNoTrackingAsync(int limit, int page)
        => WhereQueryNoTrackingAsync(f => true, limit, page);

    /// <summary>
    /// 查询最后创建的文章
    /// </summary>
    public Task<List<PostInfo>> QueryByLastNoTrackingAsync(int limit, int page)
    {
        var startIndex = limit * (page - 1);
        if (startIndex < 0) startIndex = 0;
        return Task.FromResult<List<PostInfo>>([.. context.Blogs.AsNoTracking().OrderByDescending(p => p.CreateTime).Skip(startIndex).Take(limit)]);
    }

    /// <summary>
    /// 查询自内容
    /// </summary>
    public Task<List<PostInfo>> QueryByContentNoTrackingAsync(string wordkey, int limit, int page)
        => WhereQueryNoTrackingAsync(b => b.Content.Contains(wordkey), limit, page);

    /// <summary>
    /// 查询自标签
    /// </summary>
    public Task<List<PostInfo>> QueryByTagNoTrackingAsync(string wordkey, int limit, int page)
        => WhereQueryNoTrackingAsync(b => b.Tag.Contains(wordkey), limit, page);

    /// <summary>
    /// 查询自名字
    /// </summary>
    public Task<List<PostInfo>> QueryByNameNoTrackingAsync(string wordkey, int limit, int page)
        => WhereQueryNoTrackingAsync(b => b.Name.Contains(wordkey), limit, page);

    /// <summary>
    /// 删除自Id
    /// </summary>
    public async Task DeleteByIdsAsync(params string[] ids)
    {
        List<PostInfo> blogs = [];
        foreach (var id in ids) {
            var targetBlog = await context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (targetBlog != null) {
                blogs.Add(targetBlog);
            }
        }
        foreach (var item in blogs) {
            context.Blogs.Remove(item);
        }
        await context.SaveChangesAsync();
    }

    private Task<List<PostInfo>> WhereQueryNoTrackingAsync(Expression<Func<PostInfo, bool>> query, int limit, int page)
    {
        var startIndex = limit * (page - 1);
        if (startIndex < 0) startIndex = 0;
        return Task.FromResult<List<PostInfo>>([.. context.Blogs.AsNoTracking().Where(query).Skip(startIndex).Take(limit)]);
    }

    /// <inheritdoc/>
    public async Task<int> CountByCriteriaAsync(PostCountCriteria criteria)
    {
        var allPost = QueryAllNoTracking();
        if (criteria.Tag != null) {
            allPost = allPost.Where(f => f.Tag.Contains(criteria.Tag));
        }

        if (criteria.Content != null) {
            allPost = allPost.Where(f => f.Content.Contains(criteria.Content));
        }

        if (criteria.Name != null) {
            allPost = allPost.Where(f => f.Name.Contains(criteria.Name));
        }
        return allPost.Count();
    }

    ///<inheritdoc/>
    public async Task<List<string>> TagsAsync()
    {
        HashSet<string> str = [];
        foreach (var item in QueryAllNoTracking()) {
            foreach (var item1 in item.Tag) {
                str.Add(item1);
            }
        }
        return [.. str];
    }

    #region Cache Methods

    /// <summary>
    /// 从缓存中获取全部文章
    /// </summary>
    private async Task<List<PostInfo>> AllPostInfoByCacheAsync()
    {
        var t = cache.GetOrCreateAsync<List<PostInfo>>(postsCacheKey, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1); // 1分钟缓存
            var posts = await QueryNoTrackingAsync(await CountAsync(), 1);
            posts.ToSimplePosts();
            return posts;
        });
        if (t == null) return [];
        else await t;
        return t.Result!;
    }

    /// <summary>
    /// 从缓存中查询全部数据
    /// </summary>
    public async Task<List<PostInfo>> QueryByCacheAsync(int limit, int page)
        => (await AllPostInfoByCacheAsync()).GetPageValue(limit, page) ?? [];

    /// <summary>
    /// 从缓存中查询最后创建的文章
    /// </summary>
    public async Task<List<PostInfo>> QueryByLastCacheAsync(int limit, int page)
    {
        const string queryByLastKeyName = $"Posts_{nameof(QueryByLastCacheAsync)}";
        var t = cache.GetOrCreate(queryByLastKeyName, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); // 5分
            return await QueryByLastNoTrackingAsync(limit, page);
        });
        if (t != null) await t;
        else return [];
        return t.Result;
    }

    /// <summary>
    /// 从缓存中查询自名字
    /// </summary>
    public async Task<List<PostInfo>> QueryByNameCacheAsync(string wordkey, int limit, int page)
    {
        var allcache = await AllPostInfoByCacheAsync();
        return [.. allcache.Where(f => f.Name.Contains(wordkey)).GetPageValue(limit, page)];
    }

    /// <summary>
    /// 从缓存中查询自标签
    /// </summary>
    public async Task<List<PostInfo>> QueryByTagCacheAsync(string wordkey, int limit, int page)
    {
        var allcache = await AllPostInfoByCacheAsync();
        return [.. allcache.Where(f => f.Tag.Contains(wordkey)).GetPageValue(limit, page)];
    }

    /// <summary>
    /// 从缓存中根据条件计数
    /// </summary>
    public async Task<int> CountByCriteriaCacheAsync(PostCountCriteria criteria)
    {
        var allPost = (await AllPostInfoByCacheAsync()).AsEnumerable();
        if (criteria.Tag != null) {
            allPost = allPost.Where(f => f.Tag.Contains(criteria.Tag));
        }

        if (criteria.Content != null) {
            allPost = allPost.Where(f => f.Content.Contains(criteria.Content));
        }

        if (criteria.Name != null) {
            allPost = allPost.Where(f => f.Name.Contains(criteria.Name));
        }
        return allPost.Count();
    }

    /// <summary>
    /// 从缓存中获取全部标签
    /// </summary>
    public async Task<List<string>> TagsCacheAsync()
    {
        HashSet<string> str = [];
        foreach (var item in await AllPostInfoByCacheAsync()) {
            foreach (var item1 in item.Tag) {
                str.Add(item1);
            }
        }
        return [.. str];
    }

    /// <summary>
    /// 从缓存中 获取数据条数
    /// </summary>
    public async Task<int> CountByCacheAsync()
    {
        return (await AllPostInfoByCacheAsync()).Count;
    }
    #endregion

    /// <summary>
    /// 迁移全部的yaml头
    /// </summary>
    public async Task MigrateAllYamlHeadersAsync()
    {
        foreach (var postInfo in QueryAllTracking()) {
            UpdateYamlHeaderAsync(postInfo);
        }
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 迁移markdown的yaml头
    /// </summary>
    protected override Task PreAdd(PostInfo model)
    {
        UpdateYamlHeaderAsync(model);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 迁移markdown的yaml头
    /// </summary>    
    protected override Task PreUpdateSaveChanges(PostInfo oldm, PostInfo newm)
    {
        UpdateYamlHeaderAsync(newm);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 根据<see cref="PostInfo.Content"/>更新<see cref="PostInfo.Description"/>, <see cref="PostInfo.Author"/>, <see cref="PostInfo.Tag"/>, <see cref="PostInfo.Title"/> <br/>
    /// 并回写到<see cref="PostInfo.Content"/> <br/>
    /// 如果原始内容没有Yaml头，会创建
    /// </summary>
    public void UpdateYamlHeaderAsync(PostInfo postInfo)
    {
        const string title = nameof(title);
        const string date = nameof(date);
        const string tags = nameof(tags);
        const string author = nameof(author);
        const string description = nameof(description);
        var header = new YamlHeaderParse(postInfo.Content);
        var titleHeaderValue = header.GetValue(title);
        if (titleHeaderValue == null || titleHeaderValue.Value == null) {
            header.AddHeader(title, postInfo.Name);
            postInfo.Title = postInfo.Name;
        } else {
            postInfo.Title = titleHeaderValue.Value;
        }

        var descriptionHeaderValue = header.GetValue(description);
        if (descriptionHeaderValue == null || descriptionHeaderValue.Value == null) {
            header.AddHeader(description, postInfo.Description);
        } else {
            postInfo.Description = descriptionHeaderValue.Value;
        }

        var dateHeaderValue = header.GetValue(date);
        if (dateHeaderValue == null) {
            DateTimeOffset epoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var utcTime = epoch.AddTicks(postInfo.CreateTime);
            var chinaTime = utcTime.ToOffset(TimeSpan.FromHours(8));
            header.AddHeader(date, chinaTime);
        } else {
            var yamlDateTime = dateHeaderValue.ToDateTimeValue();
            if (yamlDateTime != null) {
                postInfo.CreateTime = yamlDateTime.Value.DateTime.Ticks - DateTimeOffset.UnixEpoch.Ticks;
            }
        }

        var tagsHeaderValue = header.GetValue(tags);
        if (tagsHeaderValue == null) {
            header.AddHeader(tags, [.. postInfo.Tag]);
        } else {
            var tagArray = tagsHeaderValue.ToArrayValue();
            if (tagArray != null) {
                postInfo.Tag = [.. tagArray];
            }
        }

        var authorHeaderValue = header.GetValue(author);
        if (authorHeaderValue == null) {
            header.AddHeader(author, [.. postInfo.Author]);
        } else {
            var authorArray = authorHeaderValue.ToArrayValue();
            if (authorArray != null) {
                postInfo.Author = [.. authorArray];
            }
        }

        postInfo.Content = header.WriteToMarkdown();
    }
}
