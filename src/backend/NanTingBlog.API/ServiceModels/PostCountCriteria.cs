namespace NanTingBlog.API.ServiceModels;

/// <summary>
/// 文章计数条件类
/// </summary>
public class PostCountCriteria
{
    /// <summary>
    /// 标签
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }
}
