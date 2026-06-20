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

    /// <summary>
    /// 未通过文本
    /// </summary>
    [JsonPropertyName("failingText"), Column("failingText")]
    public string FailingText { get; set; } = "";

    /// <summary>
    /// 申请状态
    /// </summary>
    [JsonPropertyName("state"), Column("state")]
    public STATES State { get; set; } = STATES.Pending;

    /// <summary>
    /// 是否已被删除,true则是是的，false则是没有
    /// </summary>
    [JsonPropertyName("delete"), Column("delete")]
    public bool Delete { get; set; } = false;

    /// <summary>
    /// 此友链的拥有者id
    /// </summary>
    [JsonPropertyName("userId"), Column("userId")]
    [ForeignKey(nameof(User))]
    [DefaultValue("a0000000-0000-0000-0000-000000000001")]
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    public string UserId { get; set; } = "a0000000-0000-0000-0000-000000000001";

    /// <summary>
    /// 用户查询结果
    /// </summary>
    [JsonIgnore]
    public User User { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
}

/// <summary>
/// 申请状态
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum STATES
{
    /// <summary>
    /// 待定
    /// </summary>
    Pending,
    /// <summary>
    /// 通过
    /// </summary>
    Passed,
    /// <summary>
    /// 未通过
    /// </summary>
    Failing
}