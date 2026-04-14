using System.Text.Json.Serialization;

namespace NanTingBlog.API.Dtos.Blogs;
#nullable disable

/// <summary>
/// 博文信息
/// </summary>
public class BlogInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("createTime")]
    public long CreateTime { get; set; }
    [JsonPropertyName("editTime")]
    public long EditTime { get; set; }
    [JsonPropertyName("author")]
    public string Author { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
}