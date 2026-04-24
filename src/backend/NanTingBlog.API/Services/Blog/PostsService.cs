using Microsoft.EntityFrameworkCore;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Services.Db;
using System.Linq.Expressions;

namespace NanTingBlog.API.Services.Blog;

/// <summary>
/// 文章服务
/// </summary>
public class PostsService(BlogContext context) : 
    BaseQuery<PostInfo, string>(context)
{
    private readonly BlogContext context = context;

    /// <summary>
    /// 取key
    /// </summary>
    public override Expression<Func<PostInfo, string>> KeyExpression { get => field; init; } = f => f.Name;

    /// <summary>
    /// 查询全部
    /// </summary>
    public List<PostInfo> Query(int limit, int page)
        => WhereQuery(f => true, limit, page);

    /// <summary>
    /// 以指定Key查询
    /// </summary>
    public async Task<PostInfo?> QueryByKeyAsync(string key)
    {
        return await context.Blogs.AsNoTracking().FirstOrDefaultAsync(b => b.Id == key);
    }

    /// <summary>
    /// 查询自内容
    /// </summary>
    public List<PostInfo> QueryByContent(string wordkey, int limit, int page)
        => WhereQuery(b => b.Content.Contains(wordkey), limit, page);

    /// <summary>
    /// 查询自标签
    /// </summary>
    public List<PostInfo> QueryByTag(string wordkey, int limit, int page)
        => WhereQuery(b => b.Tag.Contains(wordkey), limit, page);

    /// <summary>
    /// 查询自名字
    /// </summary>
    public List<PostInfo> QueryByName(string wordkey, int limit, int page)
        => WhereQuery(b => b.Name.Contains(wordkey), limit, page);

    /// <summary>
    /// 查询全部，懒惰返回
    /// </summary>
    public IEnumerable<PostInfo> QueryAll()
    {
        foreach (var post in context.Blogs.AsNoTracking()) {
            yield return post;
        }
    }

    /// <summary>
    /// 删除自Id
    /// </summary>
    public async Task DeleteByIdsAsync(params string[] ids)
    {
        List<PostInfo> blogs = [];
        foreach (var id in ids) {
            var targetBlog = await context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if(targetBlog != null) {
                blogs.Add(targetBlog);
            }
        }
        foreach (var item in blogs) {
            context.Blogs.Remove(item);
        }
        await context.SaveChangesAsync();
    }


    /// <summary>
    /// 删除全部
    /// </summary>
    public async Task DeleteAllAsync()
    {
        var blogs = context.Blogs.ToArray();
        foreach (var item in blogs) {
            context.Blogs.Remove(item);
        }
        await context.SaveChangesAsync();
    }

    private List<PostInfo> WhereQuery(Expression<Func<PostInfo, bool>> query, int limit, int page)
    {
        var startIndex = limit * (page - 1);
        if (startIndex < 0) startIndex = 0;
        return [.. context.Blogs.AsNoTracking().Where(query).Skip(startIndex).Take(limit)];
    }
}
