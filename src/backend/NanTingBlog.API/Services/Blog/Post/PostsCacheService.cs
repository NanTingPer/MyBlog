using Microsoft.Extensions.Caching.Memory;
using NanTingBlog.API.Dtos.Blogs;
using System.Linq.Expressions;

namespace NanTingBlog.API.Services.Blog.Post;

/// <summary>
/// 带有缓存的文章查询服务，也是对<see cref="PostsService"/>的装饰
/// </summary>
/// <param name="service"></param>
/// <param name="cache"></param>
public class PostsCacheService(PostsService service, IMemoryCache cache) : IPostService
{
    private const string postsCacheKey = "AllPosts";
    /// <inheritdoc/>
    public Expression<Func<PostInfo, string>> KeyExpression { get; init; } = service.KeyExpression;

    /// <inheritdoc/>
    public Task AddAsync(PostInfo info) => service.AddAsync(info);

    /// <inheritdoc/>
    public Task<int> CountAsync() => service.CountAsync();

    /// <inheritdoc/>
    public Task DeleteAllAsync() => service.DeleteAllAsync();

    /// <inheritdoc/>
    public Task DeleteByKeyAsync(string id) => service.DeleteByKeyAsync(id);

    /// <inheritdoc/>
    public async Task<List<PostInfo>> Query(int limit, int page)
        => (await AllPostInfoByCache()).GetPageValue(limit, page) ?? [];

    /// <inheritdoc/>
    public async Task<List<PostInfo>> QueryByContent(string wordkey, int limit, int page)
    {
        var posts = await AllPostInfoByCache();
        var result = posts.Where(f => f.Content.Contains(wordkey));
        return result.ToList();
    }

    /// <inheritdoc/>
    public Task<PostInfo?> QueryByKeyNoTrackingAsync(string key)
        => service.QueryByKeyNoTrackingAsync(key);

    /// <inheritdoc/>
    public Task<PostInfo?> QueryByKeyTrackingAsync(string key)
        => service.QueryByKeyTrackingAsync(key);

    /// <inheritdoc/>
    public async Task<List<PostInfo>> QueryByLast(int limit, int page)
    {
        const string queryByLastKeyName = $"Posts_{nameof(QueryByLast)}";
        var t = cache.GetOrCreate(queryByLastKeyName, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); // 5分
            return await service.QueryByLast(limit, page);
        });
        if (t != null) await t;
        else return [];
        return t.Result;
    }

    /// <inheritdoc/>
    public async Task<List<PostInfo>> QueryByName(string wordkey, int limit, int page)
    {
        var allcache = await AllPostInfoByCache();
        return [.. allcache.Where(f => f.Name.Contains(wordkey))];
    }

    /// <inheritdoc/>
    public async Task<List<PostInfo>> QueryByTag(string wordkey, int limit, int page)
    {
        var allcache = await AllPostInfoByCache();
        return [.. allcache.Where(f => f.Tag.Contains(wordkey))];
    }

    /// <inheritdoc/>
    public Task<UpsertResult> UpdateOrAddAsync(PostInfo newInfo)
        => service.UpdateOrAddAsync(newInfo);

    private async Task<List<PostInfo>> AllPostInfoByCache()
    {
        var t = cache.GetOrCreateAsync<List<PostInfo>>(postsCacheKey, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1); // 1分钟缓存
            return await service.Query(await service.CountAsync(), 1);
        });
        if (t == null) return [];
        else await t;
        return t.Result!;
    }
}

/// <summary>
/// 扩展
/// </summary>
public static class ListPostExtension
{
    /// <summary>
    /// 获取给定页的内容
    /// </summary>
    public static List<T> GetPageValue<T>(this List<T>? list, int limit, int page)
    {
        if (list == null) return [];
        var startIndex = limit * (page - 1);
        if (startIndex < 0) startIndex = 0;
        return [.. list.Skip(startIndex).Take(limit)];
    }
}