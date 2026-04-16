using System.Text.Json.Serialization;

namespace NanTingBlog.API.Controllers;

/// <summary>
/// 控制器标准返回结果
/// </summary>
/// <typeparam name="T">结果对象</typeparam>
public class BaseResult<T>
{
    /// <summary>
    /// 内部执行结果
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; } = 200;

    /// <summary>
    /// 内部结果消息
    /// </summary>
    [JsonPropertyName("msg")]
    public string Msg { get; set; } = string.Empty;

    /// <summary>
    /// 请求要返回的结果
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; } = default(T);
}
