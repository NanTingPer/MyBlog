using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NanTingBlog.API.Services.Db;
using System.Linq.Expressions;

namespace NanTingBlog.API.Services;

/// <summary>
/// 基础查询
/// </summary>
public abstract class BaseRepository<TModel, TKey>(BlogContext context) : IBaseRepository<TModel, TKey>
    where TModel : class
{
    /// <inheritdoc/>
    public abstract Expression<Func<TModel, TKey>> KeyExpression { get; init; }
    private Func<TModel, TKey> GetKeyMethod { get => field ??= KeyExpression.Compile(); }
    private TKey GetKeyValue(TModel model) => GetKeyMethod(model);

    /// <inheritdoc/>
    public virtual async Task AddAsync(TModel info)
    {
        ArgumentNullException.ThrowIfNull(info);
        await PreAdd(info);
        context.Add(info);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 在添加操作之前调用，传入的是还未被跟踪的实体，调用完此方法后，将会调用<see cref="DbContext.AddAsync(object, CancellationToken)"/> <br></br>
    /// 此钩子用于修改TModel中属性值，不建议进行其他操作 <br></br>
    /// 仅当<see cref="AddAsync(TModel)"/>未被重写时生效
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    protected virtual async Task PreAdd(TModel model)
    {
    }

    /// <inheritdoc/>
    public virtual async Task<UpsertResult> UpdateOrAddAsync(TModel newInfo)
    {
        ArgumentNullException.ThrowIfNull(newInfo);
        var oldInfo = await context.Set<TModel>().FirstOrDefaultAsync(BuildKeyEqualExpression(newInfo));
        if (oldInfo == null) {
            await AddAsync(newInfo);
            return UpsertResult.Add;
        }
        var entry = context.Entry(oldInfo);
        entry.CurrentValues.SetValues(newInfo);

        var properties = entry.CurrentValues.Properties;
        foreach (var item in properties) {
            if (item.IsPrimaryKey()) {
                entry.Property(item.Name).IsModified = false;
            }
        }

        var names = properties.Select(f => f.Name);
        foreach (var name in ModifyPropertyModified(properties)) {
            if (names.Any(f => f == name)) {
                entry.Property(name).IsModified = false;
            }
        }
        ;
        await PreUpdateSaveChanges(oldInfo, newInfo);
        await context.SaveChangesAsync();
        return UpsertResult.Update;
    }

    /// <summary>
    /// 在更新操作的<see cref="DbContext.SaveChangesAsync(CancellationToken)"/>之前执行，传入被跟踪的对象 <br></br>
    /// 此对象的属性值已经是更新后的值，如果要将更改同步到数据库，请修改<paramref name="newm"/>的值 <br></br>
    /// 此钩子用于修改TModel中属性值，不建议进行其他操作 <br></br>
    /// 仅当<see cref="UpdateOrAddAsync(TModel)"/>未被重写时生效
    /// </summary>
    /// <returns></returns>
    protected virtual async Task PreUpdateSaveChanges(TModel oldm, TModel newm)
    {
    }

    /// <summary>
    /// 在新实体数据被更改后，你可以设置此新值是否要被更新到数据库中。<br></br>
    /// 这样可以保证IsModified=false的数据，其值为数据库的原始值 <br></br>
    /// </summary>
    /// <returns> 不需要被修改的属性名称集合 </returns>
    protected virtual List<string> ModifyPropertyModified(IReadOnlyList<IProperty> propertys)
    {
        return [];
    }

    /// <inheritdoc/>
    public async Task DeleteByKeyAsync(TKey id)
    {
        var targetBlog = await context.Set<TModel>().FirstOrDefaultAsync(BuildKeyEqualExpression(id));
        if (targetBlog == null) {
            return;
        }

        context.Set<TModel>().Remove(targetBlog);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<TModel?> QueryByKeyNoTrackingAsync(TKey key)
    {
        return await context.Set<TModel>().AsNoTracking().FirstOrDefaultAsync(BuildKeyEqualExpression(key));
    }

    /// <inheritdoc/>
    public async Task<TModel?> QueryByKeyTrackingAsync(TKey key)
    {
        return await context.Set<TModel>().FirstOrDefaultAsync(BuildKeyEqualExpression(key));
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync()
    {
        return await context.Set<TModel>().AsNoTracking().CountAsync();
    }

    private Expression<Func<TModel, bool>> BuildKeyEqualExpression(TKey keyValue)
    {
        // model => model.key == target;

        // model
        var keyParameter = KeyExpression.Parameters[0];

        // model => model.key
        var propExpression = KeyExpression.Body;
        var targetKeyValue = Expression.Constant(keyValue, typeof(TKey));

        // model => model.key == target
        var equalExpression = Expression.Equal(propExpression, targetKeyValue);
        return Expression.Lambda<Func<TModel, bool>>(equalExpression, keyParameter);
    }

    private Expression<Func<TModel, bool>> BuildKeyEqualExpression(TModel model2)
    {
        // model => model.key == model2.key
        var keyV = GetKeyValue(model2);
        return BuildKeyEqualExpression(keyV);
    }

    /// <inheritdoc/>
    public async Task DeleteAllAsync()
    {
        var blogs = context.Set<TModel>();
        var models = await blogs.ToArrayAsync();
        blogs.RemoveRange(models);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public IEnumerable<TModel> QueryAllNoTracking()
    {
        foreach (var post in context.Set<TModel>().AsNoTracking()) {
            yield return post;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<TModel> QueryAllTracking()
    {
        foreach (var post in context.Set<TModel>()) {
            yield return post;
        }
    }
}