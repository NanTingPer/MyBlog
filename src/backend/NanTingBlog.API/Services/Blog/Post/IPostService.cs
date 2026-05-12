using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.ServiceModels;

namespace NanTingBlog.API.Services.Blog.Post;

/// <summary>
/// 文章服务
/// </summary>
public interface IPostService: IPostsQueryService, IBaseRepository<PostInfo, string>
{
    /// <summary>
    /// 计数
    /// </summary>
    Task<int> CountByCriteria(PostCountCriteria criteria);

    /// <summary>
    /// 获取全部标签
    /// </summary>
    Task<List<string>> Tags();
}
