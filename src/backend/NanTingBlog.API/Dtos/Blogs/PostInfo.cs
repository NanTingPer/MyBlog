using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NanTingBlog.API.Dtos.Blogs;
#nullable disable

/// <summary>
/// 博文信息
/// </summary>
[Table("posts")]
public class PostInfo
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    [JsonPropertyName("id"), Column("id"), Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("name"), Column("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("createTime"), Column("createTime")]
    public long CreateTime { get; set; } = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks;
    [JsonPropertyName("editTime"), Column("editTime")]
    public long EditTime { get; set; } = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks;
    [JsonPropertyName("author"), Column("author")]
    public List<string> Author { get; set; } = [];
    [JsonPropertyName("content"), Column("content")]
    public string Content { get; set; } = string.Empty;
    [JsonPropertyName("tag"), Column("tag")]
    public List<string> Tag { get; set; } = [];
    [JsonPropertyName("drawingUrl"), Column("drawingUrl")]
    public string DrawingUrl { get; set; } = string.Empty;
    [JsonPropertyName("html"), Column("html")]
    public string HTML { get; set; } = string.Empty;
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释