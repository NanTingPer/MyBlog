using NanTingBlog.API.Controllers;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace NanTingBlog.API.Middlewares;

/// <summary>
/// 限流中间件
/// </summary>
public class SpeedMiddleware : BackgroundService, IMiddleware
{
    private const short maxcount = 20; // max 20/min request
    private readonly ConcurrentDictionary<string, short> countIp = []; 
    /// <summary>
    /// 执行
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var remoteIpaddr = context.Connection.RemoteIpAddress;
        if (remoteIpaddr == null) {
            await next.Invoke(context);
            return;
        }
        var ipaddrStr = remoteIpaddr.ToString();

        var newCount = countIp.AddOrUpdate(ipaddrStr, 1, 
            (_, old) => (old >= maxcount || old == -1) ? (short)-1 : (short)(old + 1));

        if(newCount == -1) { // -1就是限流了
            await ExceedSpeedWriteToResponse(context);
            return;
        }

        await next.Invoke(context);
    }

    private readonly static JsonSerializerOptions options = new JsonSerializerOptions()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static async Task ExceedSpeedWriteToResponse(HttpContext context)
    {
        if (context.Response.HasStarted) {
            return;
        } else {
            var result = new BaseResult<string>()
            {
                Code = 500,
                Data = "超出速率"
            };
            context.Response.StatusCode = 500;
            await context.Response.Body.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(result, options));
        }
    }

    /// <summary>
    /// 每分钟对访问计数器进行清空
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(ClearIpCount(stoppingToken));
    }

    private async Task ClearIpCount(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            countIp.Clear();
        }
    }
}
