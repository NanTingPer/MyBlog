namespace NanTingBlog.API;

/// <summary>
/// 应用内需要使用到httpclient的情景
/// </summary>
public class HttpClientType
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }
    private HttpClientType(string name) => Name = name;

    /// <summary>
    /// 隐式转换为string
    /// </summary>
    public static implicit operator string(HttpClientType type) => type.Name;
}
