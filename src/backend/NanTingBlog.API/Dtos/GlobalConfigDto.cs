using NanTingBlog.API.Services;
using System.Text.Json.Serialization;

namespace NanTingBlog.API.Dtos;

/// <summary>
/// 全局配置的数据传输对象，仅包含配置属性，不含业务逻辑
/// </summary>
public class GlobalConfigDto
{
    /// <summary>
    /// 博文保存路径
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual string? BlogSaveDir { get; set; } = Path.Combine(AppContext.BaseDirectory, "posts");

    /// <summary>
    /// 监听端口
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual List<string>? Ports { get; set; } = ["6999"];

    /// <summary>
    /// 每分钟的最大API访问次数
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual int? MinuteAPIMaxVisit { get; set; } = 50;

    /// <summary>
    /// 登录密码
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual string? LoginPassword { get; set; } = "qwertyuiopasdfghjklzxcvbnm123";

    /// <summary>
    /// jwt令牌颁发机构
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual string? JwtIssuer { get; set; } = "blogapi";

    /// <summary>
    /// jwt令牌受用人
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual string? JwtAudience { get; set; } = "blogapi";

    /// <summary>
    /// jwt私钥
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual string? JwtKey { get; set; } = "blogapiwertyu131i3o1p654d64f1g3h1j3k65641jzsxuhjcvguawsueqwrdsojga13wdwgsdr564";

    /// <summary>
    /// 数据库链接字符串
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual string? BlogDbConnectionString { get; set; } = @"Host=127.0.0.1:5432;Username=postgres;Password=123456;Database=blog";

    /// <summary>
    /// 邮箱验证服务的基本配置
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual MailOptions? MailOptions { get; set; } = new();

    /// <summary>
    /// 管理员的邮箱地址，当使用此地址注册账户时，会将你设置为Admin，以提供接口权限
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual string? AdminMailAddress { get; set; } = "";
}