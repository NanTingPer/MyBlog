using NanTingBlog.API.Dtos.Blogs;

namespace NanTingBlog.API.Dtos.ForntObject;
/// <summary>
/// 标签类
/// </summary>
public record class Tag
{
    /// <summary>
    /// 标签名称
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// 此标签下的文章集合
    /// </summary>
    public List<PostInfo> Posts { get; set; } = [];
}
