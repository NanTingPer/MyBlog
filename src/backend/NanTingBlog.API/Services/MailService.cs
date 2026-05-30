using MailKit.Net.Smtp;
using MimeKit;

namespace NanTingBlog.API.Services;

/// <summary>
/// 邮件服务 <br/>
/// 建议注册为单例服务
/// </summary>
public class MailService : IMailService
{
    /// <summary>
    /// 邮箱配置项
    /// </summary>
    public MailOptions Options => _fOptions?.Invoke() ?? _options!;
    private readonly MailOptions? _options;

    private readonly Func<MailOptions>? _fOptions;

    /// <summary>
    /// 使用配置参数创建邮件服务
    /// </summary>
    /// <param name="options"></param>
    public MailService(MailOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>
    /// 使用配置参数创建邮件服务（支持动态修改）
    /// </summary>
    /// <param name="foptions"></param>
    public MailService(Func<MailOptions> foptions)
    {
        _fOptions = foptions;
    }

    /// <inheritdoc/>
    public async Task<bool> SendMail(MailSendDto dto, CancellationToken cancellationToken = default)
    {
        var body = new BodyBuilder()
        {
            BodyEncoding = System.Text.Encoding.UTF8,
            TextBody = dto.Body
        };
        var message = new MimeMessage()
        {
            Body = body.ToMessageBody(),
            Subject = dto.Subject
        };
        message.From.Add(new MailboxAddress(dto.YourName, Options.MailAddress));
        message.To.AddRange(dto.ToAddresses);
        var sendUri = new Uri(Options.SendUrl);
        using var smtpClient = new SmtpClient();
        try {
            await smtpClient.ConnectAsync(sendUri.Host, sendUri.Port, cancellationToken: cancellationToken, useSsl: Options.UseSSL);
            await smtpClient.AuthenticateAsync(Options.Account, Options.AuthorizationCode, cancellationToken);
            var sendMsg = await smtpClient.SendAsync(message, cancellationToken);
            return true;
        } finally {
            await smtpClient.DisconnectAsync(true, cancellationToken);
        }
    }
}

/// <summary>
/// 邮件服务的配置
/// </summary>
public class MailOptions
{
    /// <summary>
    /// 账户名称
    /// </summary>
    public string Account { get; set; } = "";

    /// <summary>
    /// 授权码
    /// </summary>
    public string AuthorizationCode { get; set; } = "";

    /// <summary>
    /// 你的邮箱地址
    /// </summary>
    public string MailAddress { get; set; } = "";

    /// <summary>
    /// 接收右键服务器地址
    /// </summary>
    public string ReveiveUrl { get; set; } = "";

    /// <summary>
    /// 发送邮件服务器地址
    /// </summary>
    public string SendUrl { get; set; } = "";

    /// <summary>
    /// 是否使用SSL链接
    /// </summary>
    public bool UseSSL { get; set; } = true;
}

/// <summary>
/// 邮件服务抽象接口
/// </summary>
public interface IMailService
{
    /// <summary>
    /// 执行邮件发送操作
    /// </summary>
    /// <returns></returns>
    Task<bool> SendMail(MailSendDto dto, CancellationToken cancellationToken = default);
}

/// <summary>
/// 邮件发送模型
/// </summary>
public class MailSendDto
{
    /// <summary>
    /// 主题
    /// </summary>
    public string Subject { get; set; } = "无主题";

    /// <summary>
    /// 内容
    /// </summary>
    public string Body { get; set; } = "无内容";

    /// <summary>
    /// 你的名字（FromName）
    /// </summary>
    public string YourName { get; set; } = "陌生人";

    /// <summary>
    /// 目标地址
    /// </summary>
    public MailboxAddress[] ToAddresses { get; set; } = [];
}