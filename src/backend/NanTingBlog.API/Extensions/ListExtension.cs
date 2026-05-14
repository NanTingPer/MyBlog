using NanTingBlog.API.Dtos.Blogs;

namespace NanTingBlog.API.Extensions;

/// <summary>
/// List扩展
/// </summary>
public static class ListExtension
{
    /// <summary>
    /// 获取给定页的内容
    /// </summary>
    public static List<T> GetPageValue<T>(this List<T>? list, int limit, int page)
    {
        if (list == null) return [];
        var startIndex = limit * (page - 1);
        if (startIndex < 0) startIndex = 0;
        return [.. list.Skip(startIndex).Take(limit)];
    }

    /// <summary>
    /// 获取给定页的内容
    /// </summary>
    /// <param name="ie">this</param>
    /// <param name="limit">单页的数据条数</param>
    /// <param name="page">第几页</param>
    /// <returns></returns>
    public static IEnumerable<T> GetPageValue<T>(this IEnumerable<T>? ie, int limit, int page)
    {
        if (ie == null) return [];
        var startIndex = limit * (page - 1);
        if (startIndex < 0) startIndex = 0;
        return [.. ie.Skip(startIndex).Take(limit)];
    }

    /// <summary>
    /// 将postinfo的<see cref="PostInfo.Content"/>截断为20个字符
    /// </summary>
    /// <param name="posts"></param>
    public static void ToSimplePosts(this List<PostInfo> posts)
    {
        foreach (var item in posts) {
            if (item.Content.Length <= 20) {
                item.Content = item.Content[0..item.Content.Length];
            } else {
                item.Content = item.Content[0..20];
            }
        }
    }
}
