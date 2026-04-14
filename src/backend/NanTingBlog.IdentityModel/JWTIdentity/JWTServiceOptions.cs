using Microsoft.AspNetCore.Http;

namespace NanTingBlog.IdentityModel.JWTIdentity;

public class JWTServiceOptions
{
    /// <summary>
    /// 验证使用的私钥
    /// </summary>
    public Func<string>? SecurityKey { get; set; }

    /// <summary>
    /// 验证失败返回的状态码
    /// </summary>
    public Func<HttpContext, int>? UnauthorizedCode { get; set; }

    /// <summary>
    /// 验证失败要覆写Content的内容
    /// </summary>
    public Func<HttpContext, string>? UnauthorizedContent { get; set; }
}
