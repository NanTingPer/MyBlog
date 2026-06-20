using NanTingBlog.API.Dtos;
using NanTingBlog.API.Services.Identitys;
using System.Text.Json;

namespace NanTingBlog.API.Extensions;

/// <summary>
/// <see cref="HttpContext"/>的扩展方法。包含身份验证等
/// </summary>
public static class HttpContextExtension
{
    /// <summary>
    /// 获取用户的角色，如果不存在则返回null
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static List<UserRole>? GetUserRoles(this HttpContext context)
    {
        var userRoleClaim = context.User.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.USER_ROLE)!;
        if(userRoleClaim == null) {
            return null;
        }
        var userRoles = JsonSerializer.Deserialize<List<Dtos.UserRole>>(userRoleClaim.Value);
        return userRoles;
    }

    /// <summary>
    /// <see cref="CustomClaimTypes.USER_ROLE"/> 中是否包含 <see cref="UserRole.Admin"/>
    /// </summary>
    public static bool IsAdmin(this HttpContext context)
    {
        var roles = context.GetUserRoles();
        if (roles == null) {
            return false;
        }
        return roles.Contains(UserRole.Admin);
    }


    /// <summary>
    /// <see cref="CustomClaimTypes.USER_ROLE"/> 中是否包含 <see cref="UserRole.User"/>
    /// </summary>
    public static bool IsUser(this HttpContext context)
    {
        var roles = context.GetUserRoles();
        if (roles == null) {
            return false;
        }
        return roles.Contains(UserRole.User);
    }


    /// <summary>
    /// 获取用户id，通过<see cref="CustomClaimTypes.USER_ID"/>
    /// </summary>
    public static string? GetUserId(this HttpContext context)
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.USER_ID);
        if (userIdClaim == null) {
            return null;
        }
        return userIdClaim.Value;
    }
}
