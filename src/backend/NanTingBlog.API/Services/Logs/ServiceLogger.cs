
using Serilog;
using Serilog.Formatting.Json;

namespace NanTingBlog.API.Services.Logs;

/// <summary>
/// 系统日志
/// </summary>
public class ServiceLogger : BaseLog
{
    /// <summary>
    /// log_service
    /// </summary>
    /// <returns></returns>
    protected override Serilog.ILogger CreateLogger()
    {
        var logRootDir = Path.Combine(AppContext.BaseDirectory, "logs");
        return new LoggerConfiguration()
            .WriteTo.File(path: Path.Combine(logRootDir, "log_service_json_formatter.log"), formatter: new JsonFormatter(), rollingInterval: RollingInterval.Month, retainedFileCountLimit: 20)
            .WriteTo.File(path: Path.Combine(logRootDir, "log_service.log"), outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Month, retainedFileCountLimit: 20)
            .CreateLogger();
    }
}
