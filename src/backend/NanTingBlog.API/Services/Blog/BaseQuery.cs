using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NanTingBlog.API.Services.Db;
using System.Linq.Expressions;

namespace NanTingBlog.API.Services.Blog;

/// <summary>
/// 基础查询
/// </summary>
public abstract class BaseQuery<TModel, TKey>(BlogContext context)
    where TModel : class
{
    /// <summary>
    /// 返回Key的表达式
    /// </summary> 
    public abstract Expression<Func<TModel, TKey>> KeyExpression { get; init; }
    private Func<TModel, TKey> GetKeyMethod { get => field ??= KeyExpression.Compile(); }
    private TKey GetKeyValue(TModel model) => GetKeyMethod(model);

    /// <summary>
    /// 添加
    /// </summary>
    public async Task AddAsync(TModel info)
    {
        ArgumentNullException.ThrowIfNull(info);
        context.Add(info);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 更新或添加
    /// </summary>
    public async Task UpdateOrAddAsync(TModel newInfo)
    {
        ArgumentNullException.ThrowIfNull(newInfo);
        var oldInfo = await context.Set<TModel>().FirstOrDefaultAsync(BuildKeyEqualExpression(newInfo));
        if (oldInfo == null) {
            await AddAsync(newInfo);
            return;
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
        await context.SaveChangesAsync();
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

    /// <summary>
    /// 删除自Id
    /// </summary>
    public async Task DeleteByIdAsync(TKey id)
    {
        var targetBlog = await context.Set<TModel>().FirstOrDefaultAsync(BuildKeyEqualExpression(id));
        if (targetBlog == null) {
            return;
        }

        context.Set<TModel>().Remove(targetBlog);
        await context.SaveChangesAsync();
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
}
