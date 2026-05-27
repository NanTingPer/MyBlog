using Microsoft.AspNetCore.Mvc;
using NanTingBlog.API.Dtos;
using NanTingBlog.API.Services;
using NanTingBlog.API.Services.Identitys;
using NanTingBlog.API.Services.Logs;
using System.Text.Json.Serialization;

namespace NanTingBlog.API.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthenticationController(
    RSAService rsa,
    JwtService jwtService,
    UserService userService,
    UserPasswordHasher passwordHasher,
    LoginLogger logger) : ControllerBase
{
    /// <summary>
    /// 注册用户
    /// </summary>
    [HttpPost("createUser")]
    public async Task<ActionResult<BaseResult<string>>> CreateUser([FromBody] UserInput input)
    {
        var result = BaseResult<string>.Create("注册成功");
        if (input.UserName == null || input.Password == null || input.RsaId == null) {
            result.Code = 500;
            result.Data = "必填参数未填写";
            return Ok(result);
        }

        input.Password = rsa.Decrypt(input.Password, input.RsaId);
        if(input.Password == null) {
            result.Code = 500;
            result.Data = "非对称加密验证失败";
            return Ok(result);
        }

        var oldUser = await userService.GetUserByName(input.UserName);
        if(oldUser != null) {
            result.Code = 500;
            result.Data = "用户名以存在";
            return Ok(result);
        }

        var newUser = new User()
        {
            Name = input.UserName,
            Password = input.Password
        };
        try {
            newUser.Password = passwordHasher.HashPassword(newUser, input.Password);
            await userService.AddAsync(newUser);
        } catch(Exception e) {
            result.Data = "内部错误";
            result.Code = 500;
            logger.Error(e.Message + e.StackTrace);
            return Ok(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 获取Token
    /// </summary>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ActionResult<BaseResult<string>>> GetToken([FromBody] UserInput input)
    {
        var result = new BaseResult<string>();
        if (input.UserName == null || input.Password == null || input.RsaId == null) {
            result.Code = 500;
            result.Data = "必填参数未填写";
            return Ok(result);
        }
        var user = await userService.GetUserByName(input.UserName);
        if (user == null) {
            result.Code = 500;
            result.Data = "用户未找到";
            return Ok(result);
        }

        var origPassword = rsa.Decrypt(input.Password, input.RsaId);
        if (origPassword == null) {
            result.Code = 500;
            result.Data = "非对称加密验证失败";
            return Ok(result);
        }
        var verifcationResult = passwordHasher.VerifyHashedPassword(user, user.Password, origPassword);
        if (verifcationResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed) {
            result.Code = 500;
            result.Data = "身份验证失败";
            return Ok(result);
        }

        result.Data = jwtService.CreateToken(user);
        return Ok(result);
    }

    /// <summary>
    /// 获取rsa公钥
    /// </summary>
    /// <returns></returns>
    [HttpPost("public")]
    public async Task<ActionResult<BaseResult<PublicKey>>> GetPublicKey()
    {
        return Ok(BaseResult<PublicKey>.Create(rsa.GetPublicKey()));
    }

    /// <summary>
    /// 创建测试用管理员用户
    /// </summary>
    /// <returns></returns>
#if DEBUG
    [HttpPost("createTestAdminUser")]
    public async Task<ActionResult<BaseResult<string>>> CreateTestAdminUser()
    {
        var newUser = new User()
        {
            Name = "admin",
            Password = "admin123456",
            Roles = [UserRole.User, UserRole.Admin]
        };
        try {
            newUser.Password = passwordHasher.HashPassword(newUser, newUser.Password);
            await userService.UpdateOrAddAsync(newUser);
        } catch {
        }
        return Ok(BaseResult<string>.Create("Ok"));
    }
#endif
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public class UserInput
    {
        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("rsaid")]
        public string? RsaId { get; set; }
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}
