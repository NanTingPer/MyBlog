using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace NanTingBlog.IdentityModel.JWTIdentity;

/// <summary>
/// 讲此标记置于方法中，同时调用 <br/>
/// <see cref="IServiceCollectionExtension.AddJWTService(IServiceCollection, Func{NanTingBlog.IdentityModel.JWTIdentity.JWTServiceOptions}?)"/>     <br/>
/// <see cref="WebApplicationExtension.AddJWTMiddleware(WebApplication)"/>                                                                           <br/>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class JWTAttribute : Attribute
{
    private readonly static JsonWebTokenHandler handler = new JsonWebTokenHandler();
    public async Task<bool> OnAuthorizationAsync(HttpContext context, string skey)
    {
        var actionDesc = context.GetEndpoint()?.Metadata?.GetMetadata<JWTAttribute>();
        if (actionDesc == null) return true;

        var jwtStr = context.Request.Headers.Authorization.ToString();
        if (jwtStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
            jwtStr = jwtStr["Bearer ".Length..];
        }
        jwtStr = jwtStr.Trim();
        if (!await VerifyToken(jwtStr, skey)) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 验证jwt
    /// </summary>
    /// <param name="jwtStr"> jwt字符串 </param>
    /// <param name="skey"> 密钥 </param>
    /// <returns></returns>
    private static async Task<bool> VerifyToken(string jwtStr, string skey)
    {
        if (string.IsNullOrEmpty(jwtStr) || string.IsNullOrEmpty(skey)) {
            return false;
        }

        var securityKeyBytes = Encoding.UTF8.GetBytes(skey);
        var parameters = new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(securityKeyBytes),
            ClockSkew = TimeSpan.Zero
        };
        try {
            return (await handler.ValidateTokenAsync(jwtStr, parameters)).IsValid;
        } catch {
            return false;
        }
    }

    public static string CreateToken(string key)
    {
        var siKey = Encoding.UTF8.GetBytes(key);
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(siKey), SecurityAlgorithms.HmacSha256Signature);
        var jwtTokenDesc = new SecurityTokenDescriptor()
        {
            SigningCredentials = signingCredentials,
            Expires = DateTime.UtcNow.AddMinutes(10), //todo 测试2秒
            NotBefore = DateTime.UtcNow
        };
        return handler.CreateToken(jwtTokenDesc);
    }
}
