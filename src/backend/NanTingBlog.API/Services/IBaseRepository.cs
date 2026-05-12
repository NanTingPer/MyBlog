using Microsoft.EntityFrameworkCore;
using NanTingBlog.API.Dtos.Blogs;
using System.Linq.Expressions;

namespace NanTingBlog.API.Services;

/// <summary>
/// 基础查询接口
/// </summary>
public interface IBaseRepository<TModel, TKey>
    where TModel : class
{
    /// <summary>
    /// 返回Key的表达式
    /// </summary> 
    Expression<Func<TModel, TKey>> KeyExpression { get; init; }
    /// <summary>
    /// 进行添加操作
    /// </summary>
    Task AddAsync(TModel info);
    /// <summary>
    /// 更新或添加
    /// </summary>
    Task<UpsertResult> UpdateOrAddAsync(TModel newInfo);
    /// <summary>
    /// 删除自主键
    /// </summary>
    Task DeleteByKeyAsync(TKey id);
    /// <summary>
    /// 删除全部
    /// </summary>
    Task DeleteAllAsync();
    /// <summary>
    /// 以指定主键查询
    /// </summary>
    Task<TModel?> QueryByKeyNoTrackingAsync(TKey key);
    /// <summary>
    /// 以指定主键查询
    /// </summary>
    Task<TModel?> QueryByKeyTrackingAsync(TKey key);
    /// <summary>
    /// 表中的数据条数
    /// </summary>
    Task<int> CountAsync();

    /// <summary>
    /// 查询全部，懒惰返回表中数据，不跟踪
    /// </summary>
    IEnumerable<TModel> QueryAllNoTracking();
    /// <summary>
    /// 查询全部，懒惰返回表中数据，跟踪
    /// </summary>
    IEnumerable<TModel> QueryAllTracking();

}
/// <summary>
/// 任务结果
/// </summary>
public enum UpsertResult
{
    /// <summary>
    /// 执行了添加操作
    /// </summary>
    Add,
    /// <summary>
    /// 执行了更新操作
    /// </summary>
    Update
}