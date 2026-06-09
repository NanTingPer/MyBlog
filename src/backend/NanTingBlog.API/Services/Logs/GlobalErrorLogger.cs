using Serilog;
using Serilog.Formatting.Json;

namespace NanTingBlog.API.Services.Logs;

/// <summary>
/// 全局错误日志
/// </summary>
public class GlobalErrorLogger : BaseLog
{
    /// <summary>
    /// 唯一实例
    /// </summary>
    public readonly static Serilog.ILogger logger = new LoggerConfiguration()
            .WriteTo.File(path: Path.Combine(Path.Combine(AppContext.BaseDirectory, "logs"), "log_err_json_formatter.log"), formatter: new JsonFormatter(), rollingInterval: RollingInterval.Month, retainedFileCountLimit: 20)
            .WriteTo.File(path: Path.Combine(Path.Combine(AppContext.BaseDirectory, "logs"), "log_err.log"), outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Month, retainedFileCountLimit: 20)
            .CreateLogger();

    static GlobalErrorLogger()
    {
        AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
    }

    private static void CurrentDomain_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
    {
        logger.Error(e.Exception.Message);
    }

    private GlobalErrorLogger() { }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override Serilog.ILogger CreateLogger()
    {
        return logger;
    }
}
