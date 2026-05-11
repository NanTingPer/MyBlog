using NanTingBlog.API.Dtos.Blogs;

namespace NanTingBlog.API.Services.Blog.Post;

/// <summary>
/// 文章服务
/// </summary>
public interface IPostService: IPostsQueryService, IBaseRepository<PostInfo, string>
{
}
