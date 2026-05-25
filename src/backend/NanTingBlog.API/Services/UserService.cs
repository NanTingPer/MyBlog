using Microsoft.EntityFrameworkCore.Metadata;
using NanTingBlog.API.Dtos;
using NanTingBlog.API.Services.Db;
using System.Linq.Expressions;

namespace NanTingBlog.API.Services;

/// <summary>
/// 用户服务
/// </summary>
public class UserService(BlogContext context) : BaseRepository<User, string>(context)
{
    /// <inheritdoc/>
    public override Expression<Func<User, string>> KeyExpression { get; init; } = u => u.Id;

    /// <inheritdoc/>
    protected override List<string> ModifyPropertyModified(IReadOnlyList<IProperty> propertys)
    {
        return [nameof(User.Id), nameof(User.CreateTime)];
    }

    /// <summary>
    /// 使用用户名获取用户
    /// </summary>
    /// <returns></returns>
    public async Task<User?> GetUserByName(string userName)
    {
        return (await WhereQueryNoTrackingAsync(f => f.Name == userName)).FirstOrDefault() ?? null;
    }
}
