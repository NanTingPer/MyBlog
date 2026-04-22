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
    [Key]
    [Column("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [Column("createTime")]
    [JsonPropertyName("createTime")]
    public long CreateTime { get; set; } = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks;
    [Column("editTime")]
    [JsonPropertyName("editTime")]
    public long EditTime { get; set; } = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks;
    [Column("author")]
    [JsonPropertyName("author")]
    public List<string> Author { get; set; } = [];
    [Column("content")]
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    [Column("tag")]
    [JsonPropertyName("tag")]
    public List<string> Tag { get; set; } = [];
    [Column("drawingUrl")]
    [JsonPropertyName("drawingUrl")]
    public string DrawingUrl { get; set; } = string.Empty;
    [Column("html")]
    [JsonPropertyName("html")]
    public string HTML { get; set; } = string.Empty;
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释