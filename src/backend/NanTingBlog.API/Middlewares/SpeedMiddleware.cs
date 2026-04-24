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
    private readonly Channel<string> pendingIp = Channel.CreateUnbounded<string>();
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
        if (countIp.TryGetValue(ipaddrStr, out var count)) {
            if(count > maxcount) {
                await ExceedSpeedWriteToResponse(context);
                return;
            }
        }

        if (!pendingIp.Writer.TryWrite(ipaddrStr)) {
            await ExceedSpeedWriteToResponse(context);
            return;
        }

        await next.Invoke(context);

        //if (remoteIpaddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
        //    pendingIp.Writer.TryWrite(remoteIpaddr.ToString());
        //} else if (remoteIpaddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) { // 不支持ipv6
        //    if (context.Response.HasStarted) {
        //        return;
        //    } else {
        //        var result = new BaseResult<string>()
        //        {
        //            Code = 500,
        //            Data = "不支持Ipv6访问"
        //        };
        //        context.Response.StatusCode = 500;
        //        context.Response.Body.Write(JsonSerializer.SerializeToUtf8Bytes(result));
        //    }
        //}
    }

    private static JsonSerializerOptions options = new JsonSerializerOptions()
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
        //Task.Run(...) 这里运行一个阻塞队列，对字典中的value进行自增操作
        //Task.Factory.StartNew(async () => {

        //}).Unwrap();
        
        await Task.WhenAll(ClearIpCount(stoppingToken), HandleCount(stoppingToken));
    }

    private async Task ClearIpCount(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested) {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            countIp.Clear();
        }
    }

    private async Task HandleCount(CancellationToken stoppingToken)
    {
        await foreach (var ip in pendingIp.Reader.ReadAllAsync(stoppingToken)) {
            countIp.AddOrUpdate(ip, 1, (k, o) => (short)(o + 1));
        }
    }
}
