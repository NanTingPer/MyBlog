using Microsoft.AspNetCore.Identity;
using NanTingBlog.API.Dtos;

namespace NanTingBlog.API.Services.Identitys;

/// <summary>
/// 用户密码哈希 / 验证类
/// </summary>
public class UserPasswordHasher : PasswordHasher<User>
{
}
