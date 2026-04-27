using Microsoft.AspNetCore.Mvc;
using NanTingBlog.API.Services;
using NanTingBlog.IdentityModel.JWTIdentity;
using System.Text.Json.Serialization;

namespace NanTingBlog.API.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthenticationController(GlobalConfigService configService) : ControllerBase
{
    /// <summary>
    /// 获取Token
    /// </summary>
    /// <returns></returns>
    [HttpPost("getToken")]
    public ActionResult<BaseResult<string>> GetToken([FromBody] GetTokenInput input)
    {
        var result = new BaseResult<string>()
        {
            Code = 200
        };
        if((input.Password ?? "").Equals(configService.LoginPassword)) {
            try {
                var token = JWTAttribute.CreateToken(configService.LoginPassword);
                result.Data = token;
                return Ok(result);
            } catch (ArgumentOutOfRangeException) {
                result.Code = 400;
                result.Data = "密码长度不足256字节，请更改.";
                return result;
            } catch {
                return Unauthorized();
            }
        }

        return Unauthorized();
    }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public class GetTokenInput
    {
        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}
