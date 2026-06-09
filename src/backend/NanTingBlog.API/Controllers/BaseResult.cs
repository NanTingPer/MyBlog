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

    /// <summary>
    /// 使用给定数据创建一个BaseResult
    /// </summary>
    /// <returns></returns>
    public static BaseResult<T> Create(T data)
    {
        return new BaseResult<T>()
        {
            Data = data
        };
    }

    /// <summary>
    /// 创建一个错误请求信息
    /// </summary>
    /// <param name="msg"> 错误消息 </param>
    /// <param name="code"> 错误代码 </param>
    /// <returns></returns>
    public static BaseResult<T> CreateError(string msg, int code = 500)
    {
        var r = new BaseResult<T>();
        r.Msg = msg;
        r.Code = code;
        return r;
    }
}
