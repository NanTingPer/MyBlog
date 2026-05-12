using Microsoft.EntityFrameworkCore;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.ServiceModels;
using NanTingBlog.API.Services.Db;
using System.Linq.Expressions;

namespace NanTingBlog.API.Services.Blog.Post;

/// <summary>
/// 文章服务
/// </summary>
public class PostsService(BlogContext context) : BaseRepository<PostInfo, string>(context), IPostService
{
    private readonly BlogContext context = context;

    /// <summary>
    /// 取key
    /// </summary>
    public override Expression<Func<PostInfo, string>> KeyExpression { get => field; init; } = f => f.Id;

    /// <summary>
    /// 查询全部
    /// </summary>
    public Task<List<PostInfo>> QueryNoTracking(int limit, int page)
        => WhereQueryNoTracking(f => true, limit, page);

    /// <summary>
    /// 查询最后创建的文章
    /// </summary>
    public Task<List<PostInfo>> QueryByLastNoTracking(int limit, int page)
    {
        var startIndex = limit * (page - 1);
        if (startIndex < 0) startIndex = 0;
        return Task.FromResult<List<PostInfo>>([.. context.Blogs.AsNoTracking().OrderByDescending(p => p.CreateTime).Skip(startIndex).Take(limit)]);
    }

    /// <summary>
    /// 查询自内容
    /// </summary>
    public Task<List<PostInfo>> QueryByContent(string wordkey, int limit, int page)
        => WhereQueryNoTracking(b => b.Content.Contains(wordkey), limit, page);

    /// <summary>
    /// 查询自标签
    /// </summary>
    public Task<List<PostInfo>> QueryByTagNoTracking(string wordkey, int limit, int page)
        => WhereQueryNoTracking(b => b.Tag.Contains(wordkey), limit, page);

    /// <summary>
    /// 查询自名字
    /// </summary>
    public Task<List<PostInfo>> QueryByName(string wordkey, int limit, int page)
        => WhereQueryNoTracking(b => b.Name.Contains(wordkey), limit, page);

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

    private Task<List<PostInfo>> WhereQueryNoTracking(Expression<Func<PostInfo, bool>> query, int limit, int page)
    {
        var startIndex = limit * (page - 1);
        if (startIndex < 0) startIndex = 0;
        return Task.FromResult<List<PostInfo>>([.. context.Blogs.AsNoTracking().Where(query).Skip(startIndex).Take(limit)]);
    }

    /// <inheritdoc/>
    public Task<int> CountByCriteria(PostCountCriteria criteria)
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
        return Task.FromResult<int>(allPost.Count());
    }

    ///<inheritdoc/>
    public Task<List<string>> Tags()
    {
        HashSet<string> str = [];
        foreach (var item in QueryAllNoTracking()) {
            foreach (var item1 in item.Tag) {
                str.Add(item1);
            }
        }
        return Task.FromResult(str.ToList());
    }
}
