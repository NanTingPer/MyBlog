using Microsoft.AspNetCore.Authorization;
using NanTingBlog.API.Dtos;
using System.Text.Json;

namespace NanTingBlog.API.Services.Identitys;

/// <summary>
/// 用户验证参数请求
/// </summary>
public class UserRoleRequirement(params UserRole[] roles) : IAuthorizationRequirement
{
    /// <summary>
    /// 角色要求
    /// </summary>
    public List<UserRole> Roles { get; init; } = [.. roles];
}

/// <summary>
/// 用户验证处理器
/// </summary>
public class UserAuthorizationHandler : AuthorizationHandler<UserRoleRequirement>
{
    /// <summary>
    /// 认证请求
    /// </summary>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRoleRequirement requirement)
    {
        await Task.CompletedTask;
        var userRoleString = context.User.FindFirst(CustomClaimTypes.USER_ROLE)?.Value;
        if (userRoleString == null) {
            return;
        }

        var roles = JsonSerializer.Deserialize<List<UserRole>>(userRoleString) ?? [];

        if (roles.Any(f => requirement.Roles.Any(f1 => f == f1))) {
            context.Succeed(requirement);
            return;
        }
    }
}