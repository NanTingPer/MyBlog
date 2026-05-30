using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NanTingBlog.API.Dtos;

/// <summary>
/// 用户实体
/// </summary>
[Table("user")]
public class User
{
    /// <summary>
    /// key
    /// </summary>
    [JsonPropertyName("id"), Column("id"), Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 用户名称
    /// </summary>
    [JsonPropertyName("name"), Column("name"), Description("用户名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 用户密码
    /// </summary>
    [JsonPropertyName("password"), Column("password"), Description("用户密码")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 用户创建时间，1970年1月1日开始的tick数
    /// </summary>
    [JsonPropertyName("createTime"), Column("createTime"), Description("创建时间")]
    public long CreateTime { get; set; } = DateTime.UtcNow.Ticks - DateTimeOffset.UnixEpoch.Ticks;
    /// <summary>
    /// 用户邮件地址
    /// </summary>
    [JsonPropertyName("mailAddress"), Column("mailAddress"), Description("用户邮箱地址")]
    public string MailAddress { get; set; } = "";

    /// <summary>
    /// 用户角色
    /// </summary>
    [JsonPropertyName("roles"), Column("roles"), Description("用户角色")]
    public List<UserRole> Roles { get; set; } = [UserRole.User];
}

/// <summary>
/// 用户角色
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    /// <summary>
    /// 管理员
    /// </summary>
    Admin,
    /// <summary>
    /// 普通用户
    /// </summary>
    User
}