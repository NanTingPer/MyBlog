using NanTingBlog.API.Dtos.Blogs;

namespace NanTingBlog.API.Services.Blog.Post;

/// <summary>
/// 文章查询抽象                                 <br/>
/// 用于将数据库查询与缓存结合，而不编写重复代码 <br/>
/// </summary>
public interface IPostsQueryService
{
    /// <summary>
    /// 查询全部数据
    /// </summary>
    Task<List<PostInfo>> QueryNoTracking(int limit, int page);
    /// <summary>
    /// 查询最后创建的文章
    /// </summary>
    Task<List<PostInfo>> QueryByLastNoTracking(int limit, int page);
    /// <summary>
    /// 查询自内容
    /// </summary>
    Task<List<PostInfo>> QueryByContent(string wordkey, int limit, int page);
    /// <summary>
    /// 查询自标签
    /// </summary>
    Task<List<PostInfo>> QueryByTagNoTracking(string wordkey, int limit, int page);
    /// <summary>
    /// 查询自名字
    /// </summary>
    Task<List<PostInfo>> QueryByName(string wordkey, int limit, int page);
}
