using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace NanTingBlog.API.Utils;

/// <summary>
/// 深度遍历以更新一个对象值
/// </summary>
public class DeepUpdater<TModel>
{
    private static readonly Dictionary<Type, Action<object, object>> _cache = [];
    private static readonly Action<object, object> _final_update_action;
    static DeepUpdater()
    {
        var modelType = typeof(TModel);
        _final_update_action = RecursionCreateAssignExpression();
    }

    /// <summary>
    /// 将<paramref name="source"/>的属性值赋值给<paramref name="target"/>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public void Update(TModel source, TModel target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        _final_update_action(source, target);
    }

    /// <summary>
    /// 获取此类型的可写属性
    /// </summary>
    /// <returns></returns>
    private static PropertyInfo[] WritableProperties(Type type, BindingFlags bf = BindingFlags.Instance | BindingFlags.Public)
    {
        return type.GetProperties(bf).Where(prop => prop.CanRead && prop.CanWrite && prop.GetIndexParameters().Length == 0).ToArray();
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(decimal)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(Guid)
               || type == typeof(TimeSpan)
               || Nullable.GetUnderlyingType(type) != null;
    }

    /// <summary>
    /// 递归获取给定属性内中，为引用类型属性的类型
    /// </summary>
    /// <param name="info">属性类型为引用类型的属性信息</param>
    /// <param name="dealTypes"> 已被识别过的类型 </param>
    /// <param name="namespaceStart"> 只返回给定名称空间开头的类型，默认返回 System 等系统类型 <br></br> 一般这种类型不包含任何业务，因此不是必须的  </param>
    /// <returns></returns>
    private static List<Type> RecursionGetPropRefType(Type info, HashSet<Type>? dealTypes = null, string? namespaceStart = null)
    {
        if (IsSimpleType(info)) return [];
        dealTypes ??= [];
        List<Type> retList = [];

        var props = WritableProperties(info);
        var simpleTypeInfos = props.Where(p => IsSimpleType(p.PropertyType));
        var notSimpleTypeInfos = props.Except(simpleTypeInfos);

        // 给定属性的类型是否是给定名称空间的。
        bool isNamespace(PropertyInfo f) => (namespaceStart == null || (f.PropertyType.Namespace != null && f.PropertyType.Namespace.StartsWith(namespaceStart)));
        foreach (var refProp in notSimpleTypeInfos.Where(f => f.PropertyType.IsClass && isNamespace(f))) {
            var refPropType = refProp.PropertyType;
            if (dealTypes.Contains(refPropType)) continue;
            dealTypes.Add(refPropType);
            retList.Add(refPropType);
            retList.AddRange(RecursionGetPropRefType(refPropType, dealTypes));
        }
        return retList;
    }

    private static void AddRange(IList source, IList target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        target.Clear();
        foreach (var item in source) {
            target.Add(item);
        }
    }

    private static Action<object, object> RecursionCreateAssignExpression()
    {
        var modelType = typeof(TModel);
        var refPropTypes = RecursionGetPropRefType(modelType, namespaceStart: modelType.Namespace!.Split(".")[0]);
        foreach (var type in refPropTypes) {
            _cache[type] = CreateAssignExpressionOneType(type);
        }
        _cache[modelType] = CreateAssignExpressionOneType(modelType);

        var props = WritableProperties(modelType);
        var simpleTypeInfos = props.Where(p => IsSimpleType(p.PropertyType));
        var notSimpleTypeInfos = props.Except(simpleTypeInfos);

        var o_sourceParameter = Expression.Parameter(typeof(object), "source");
        var o_targetParameter = Expression.Parameter(typeof(object), "target");
        var sourceParameter = Expression.Convert(o_sourceParameter, modelType);
        var targetParameter = Expression.Convert(o_targetParameter, modelType);

        List<Expression> block = [];
        var updateInfo = typeof(DeepUpdater<TModel>).GetMethod(nameof(Update), BindingFlags.Static | BindingFlags.NonPublic);
        var addRangeMethod = typeof(DeepUpdater<TModel>).GetMethod(nameof(AddRange), BindingFlags.NonPublic | BindingFlags.Static);

        foreach (var propertyInfo in notSimpleTypeInfos) {
            var sourcePropExp = Expression.Property(sourceParameter, propertyInfo);
            var targetPropExp = Expression.Property(targetParameter, propertyInfo);

            // target is IList
            var ifArray = Expression.TypeIs(targetPropExp, typeof(IList));
            var targetAsList = Expression.Convert(targetPropExp, typeof(IList));
            var sourceAsList = Expression.Convert(sourcePropExp, typeof(IList));

            // AddRange(source, target);
            var then = Expression.Call(null, addRangeMethod!, sourceAsList, targetAsList);
            
            // Update(source, target);
            var @else = Expression.Call(updateInfo!, 
                Expression.Convert(sourcePropExp, typeof(object)),  
                Expression.Convert(targetPropExp, typeof(object)));

            // if(target is IList)<br />
            // AddRange(s, t) <br />
            // else               <br />
            // Update(s, t)   <br />
            var ifThenElse = Expression.IfThenElse(ifArray, then, @else);

            // source == null
            var sourceIsNull = Expression.Equal(sourcePropExp, Expression.Constant(null));
            // target == null
            var targetIsNull = Expression.Equal(targetPropExp, Expression.Constant(null));

            // if (!(source == null and target == null))
            var andNull = Expression.IsFalse(Expression.And(sourceIsNull, targetIsNull));

            // if(!(source == null and target == null)){ <br /> 
            //      if(target is IList)<br />                   
            //          AddRange(s, t) <br />                   
            //      else               <br />                   
            //          Update(s, t)   <br />                   
            // }
            var f = Expression.IfThen(andNull, ifThenElse);
            block.Add(f);
        }
        block.Add(Expression.Call(updateInfo!, o_sourceParameter, o_targetParameter));
        var body = Expression.Block(block);
        return Expression.Lambda<Action<object, object>>(body, o_sourceParameter, o_targetParameter).Compile();
    }

    private static void Update(object source, object target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        if (_cache.TryGetValue(source.GetType(), out var action)) {
            action.Invoke(source, target);
        }
    }

    /// <summary>
    /// 为单个类型类型生成赋值表达式，类型过滤为 <see cref="IsSimpleType(Type)"/> 返回true，参数为: (source, target)
    /// </summary>
    /// <param name="type"></param>
/*    /// <param name="assignExpBuilder"> 第一个表达式为target, 第二个表达式为source </param>
    /// <param name="propertyFilter"> 此属性是否应该被赋值 </param>*/
    private static Action<object, object> CreateAssignExpressionOneType(Type type/*, 
        Func<Expression, Expression, Expression>? assignExpBuilder = null,
        Func<PropertyInfo, bool>? propertyFilter = null*/)
    {
        var props = WritableProperties(type);
        var simpleTypeInfos = props.Where(p => IsSimpleType(p.PropertyType));
        //var notSimpleTypeInfos = props.Except(simpleTypeInfos);

        // 生成赋值表达式
        var o_sourceParameter = Expression.Parameter(typeof(object), "source");
        var o_targetParameter = Expression.Parameter(typeof(object), "target");

        var sourceParameter = Expression.Convert(o_sourceParameter, type);
        var targetParameter = Expression.Convert(o_targetParameter, type);

        List<Expression> assignList = [];
        foreach (var info in simpleTypeInfos) {
            // source.info
            var leftExp = Expression.Property(sourceParameter, info); // 左值
            // target.info
            var rightExp = Expression.Property(targetParameter, info); // 右值
            // target.info = source.info
            var assignExp = /*assignExpBuilder?.Invoke(rightExp, leftExp) ?? */Expression.Assign(rightExp, leftExp);
            // target.info == null
            var nullExp = Expression.Constant(null);
            var equalNullExp = Expression.Equal(leftExp, nullExp);

            // if != null then target.info = source.info  <br />
            // body
            var ifThen = Expression.IfThen(Expression.IsFalse(equalNullExp), assignExp);
            assignList.Add(ifThen);
        }
        var body = Expression.Block(assignList);
        return Expression.Lambda<Action<object, object>>(body, o_sourceParameter, o_targetParameter).Compile();
    }
}
