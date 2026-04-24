using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NanTingBlog.API.Dtos.Blogs;

/// <summary>
/// 友联
/// </summary>
[Table("friendslink")]
public class Friendslink
{
    /// <summary>
    /// 主键
    /// </summary>
    [JsonPropertyName("id"), Column("id"), Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name"), Column("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 跳转链接
    /// </summary>
    [JsonPropertyName("url"), Column("url")]
    public string Url { get; set; } = "";

    /// <summary>
    /// 格言
    /// </summary>
    [JsonPropertyName("dictum"), Column("dictum")]
    public string Dictum { get; set; } = "";

    /// <summary>
    /// 头像url
    /// </summary>
    [JsonPropertyName("avatar"), Column("avatar")]
    public string Avatar { get; set; } = "";

    /// <summary>
    /// 创建时间戳
    /// </summary>
    [JsonPropertyName("createUnixEpochTick"), Column("createUnixEpochTick"), Description("1970年1月1号到创建时间的戳")]
    public long CreateUnixEpochTick { get; private set; } = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks;

    /// <summary>
    /// 创建时间字符串
    /// </summary>
    [JsonPropertyName("createTime"), Column("createTime")]
    public string CreateTime { get; private set; } = DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss");
}