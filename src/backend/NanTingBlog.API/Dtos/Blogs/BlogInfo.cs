using System.Text.Json.Serialization;

namespace NanTingBlog.API.Dtos.Blogs;
#nullable disable

/// <summary>
/// 博文信息
/// </summary>
public class BlogInfo
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("createTime")]
    public long CreateTime { get; set; }
    [JsonPropertyName("editTime")]
    public long EditTime { get; set; }
    [JsonPropertyName("author")]
    public List<string> Author { get; set; } = [];
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("tag")]
    public List<string> Tag { get; set; } = [];
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释