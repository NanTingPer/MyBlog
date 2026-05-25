using NanTingBlog.API.Dtos;

namespace NanTingBlog.API.Services.Identitys;

/// <summary>
/// 策略
/// </summary>
public class Policy
{
    /// <summary>
    /// 管理员策略
    /// </summary>
    public static UserRoleRequirement AdminPolicyRequirement { get; } = new UserRoleRequirement(UserRole.Admin, UserRole.User);

    /// <summary>
    /// 用户策略
    /// </summary>
    public static UserRoleRequirement UserPolicyRequirement { get; } = new UserRoleRequirement(UserRole.User);
}
