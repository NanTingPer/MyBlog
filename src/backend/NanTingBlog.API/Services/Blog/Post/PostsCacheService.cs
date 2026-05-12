using Microsoft.Extensions.Caching.Memory;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.ServiceModels;
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
    public async Task<int> CountByCriteria(PostCountCriteria criteria)
    {
        var allPost = (await AllPostInfoByCache()).AsEnumerable();
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

    /// <inheritdoc/>
    public Task DeleteAllAsync() => service.DeleteAllAsync();

    /// <inheritdoc/>
    public Task DeleteByKeyAsync(string id) => service.DeleteByKeyAsync(id);

    /// <inheritdoc/>
    public async Task<List<PostInfo>> QueryNoTracking(int limit, int page)
        => (await AllPostInfoByCache()).GetPageValue(limit, page) ?? [];

    /// <inheritdoc/>
    public IEnumerable<PostInfo> QueryAllNoTracking()
    {
        return service.QueryAllNoTracking();
    }

    /// <inheritdoc/>
    public IEnumerable<PostInfo> QueryAllTracking()
    {
        return service.QueryAllNoTracking();
    }

    /// <inheritdoc/>
    public async Task<List<PostInfo>> QueryByContent(string wordkey, int limit, int page)
    {
        var posts = await AllPostInfoByCache();
        var result = posts.Where(f => f.Content.Contains(wordkey)).GetPageValue(limit, page);
        return [.. result];
    }

    /// <inheritdoc/>
    public Task<PostInfo?> QueryByKeyNoTrackingAsync(string key)
        => service.QueryByKeyNoTrackingAsync(key);

    /// <inheritdoc/>
    public Task<PostInfo?> QueryByKeyTrackingAsync(string key)
        => service.QueryByKeyTrackingAsync(key);

    /// <inheritdoc/>
    public async Task<List<PostInfo>> QueryByLastNoTracking(int limit, int page)
    {
        const string queryByLastKeyName = $"Posts_{nameof(QueryByLastNoTracking)}";
        var t = cache.GetOrCreate(queryByLastKeyName, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); // 5分
            return await service.QueryByLastNoTracking(limit, page);
        });
        if (t != null) await t;
        else return [];
        return t.Result;
    }

    /// <inheritdoc/>
    public async Task<List<PostInfo>> QueryByName(string wordkey, int limit, int page)
    {
        var allcache = (await AllPostInfoByCache());
        return [.. allcache.Where(f => f.Name.Contains(wordkey)).GetPageValue(limit, page)];
    }

    /// <inheritdoc/>
    public async Task<List<PostInfo>> QueryByTagNoTracking(string wordkey, int limit, int page)
    {
        var allcache = await AllPostInfoByCache();
        return [.. allcache.Where(f => f.Tag.Contains(wordkey)).GetPageValue(limit, page)];
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> Tags()
    {
        HashSet<string> str = [];
        foreach (var item in await AllPostInfoByCache()) {
            foreach (var item1 in item.Tag) {
                str.Add(item1);
            }
        }
        return [.. str];
    }

    /// <inheritdoc/>
    public Task<UpsertResult> UpdateOrAddAsync(PostInfo newInfo)
        => service.UpdateOrAddAsync(newInfo);

    private async Task<List<PostInfo>> AllPostInfoByCache()
    {
        var t = cache.GetOrCreateAsync<List<PostInfo>>(postsCacheKey, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1); // 1分钟缓存
            return await service.QueryNoTracking(await service.CountAsync(), 1);
        });
        if (t == null) return [];
        else await t;
        return t.Result!;
    }
}

/// <summary>
/// 扩展
/// </summary>
public static class ListExtension
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

    /// <summary>
    /// 获取给定页的内容
    /// </summary>
    /// <param name="ie">this</param>
    /// <param name="limit">单页的数据条数</param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public static IEnumerable<T> GetPageValue<T>(this IEnumerable<T>? ie, int limit, int page)
    {
        if (ie == null) return [];
        var startIndex = limit * (page - 1);
        if (startIndex < 0) startIndex = 0;
        return [.. ie.Skip(startIndex).Take(limit)];
    }
}