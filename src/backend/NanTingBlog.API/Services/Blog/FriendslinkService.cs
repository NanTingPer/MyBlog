using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Services.Db;
using System.Linq.Expressions;

namespace NanTingBlog.API.Services.Blog;

/// <summary>
/// 友链服务
/// </summary>
public class FriendslinkService(BlogContext context)
    : BaseQuery<Friendslink, string>(context)
{
    private readonly BlogContext context = context;

    /// <summary>
    /// key
    /// </summary>
    public override Expression<Func<Friendslink, string>> KeyExpression { get; init; } = f => f.Id;

    /// <summary>
    /// 获取全部友链
    /// </summary>
    public List<Friendslink> GetAll()
    {
        return [.. context.Set<Friendslink>().AsNoTracking()];
    }

    /// <inheritdoc/>
    protected override List<string> ModifyPropertyModified(IReadOnlyList<IProperty> propertys)
    {
        return [nameof(Friendslink.CreateTime), nameof(Friendslink.CreateUnixEpochTick)];
    }
}
