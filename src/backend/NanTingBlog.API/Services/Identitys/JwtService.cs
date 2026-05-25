using Microsoft.IdentityModel.Tokens;
using NanTingBlog.API.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace NanTingBlog.API.Services.Identitys;

/// <summary>
/// JsonWebToken创建服务
/// </summary>
public class JwtService(GlobalConfigService configService)
{
    /// <summary>
    /// JWT验证参数
    /// </summary>
    public TokenValidationParameters JwtValidationParameters => new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = configService.JwtIssuer,
        ValidAudience = configService.JwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configService.JwtKey))
    };

    private readonly static JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
    /// <summary>
    /// 根据用户信息创建令牌
    /// </summary>
    public string CreateToken(User user)
    {
        var roleClaim = new Claim(CustomClaimTypes.USER_ROLE, JsonSerializer.Serialize(user.Roles));
        var ssk = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configService.JwtKey));
        var scredntial = new SigningCredentials(ssk, SecurityAlgorithms.HmacSha256);

        var jwtSecurity = new JwtSecurityToken(
            issuer: configService.JwtIssuer, 
            audience: configService.JwtAudience, 
            claims: [roleClaim], 
            signingCredentials: scredntial,
            expires: DateTime.UtcNow.AddHours(2));
        return handler.WriteToken(jwtSecurity);
    }
}
