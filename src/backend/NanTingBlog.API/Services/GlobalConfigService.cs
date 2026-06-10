using NanTingBlog.API.Dtos;
using NanTingBlog.API.Services.Logs;
using NanTingBlog.API.Utils;
using System.Text;
using System.Text.Json;

namespace NanTingBlog.API.Services;

/// <summary>
/// 全局配置服务，继承 <see cref="GlobalConfigDto"/>，在属性 setter 中自动持久化
/// </summary>
public class GlobalConfigService : GlobalConfigDto
{
    private readonly static DeepUpdater<GlobalConfigDto> _update;
    private readonly BaseLog _logger;

    static GlobalConfigService()
    {
        _update = new DeepUpdater<GlobalConfigDto>();
    }

    /// <summary> </summary>
    public GlobalConfigService(ServiceLogger logger)
    {
        _logger = logger;
        if (!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }

        if (!File.Exists(FullPath)) {
            using (var fileStream = File.Create(FullPath)){ }
            Save();
        }
    }

    /// <summary>
    /// 配置的全路径，精确到文件全名称 如  global.conf
    /// </summary>
    public static string FullPath => Path.Combine(AppContext.BaseDirectory, "configs", "global.conf");
    /// <summary>
    /// 配置目录路径
    /// </summary>
    public static readonly string dirPath = Path.Combine(AppContext.BaseDirectory, "configs");
    /// <summary>
    /// 配置文件名称
    /// </summary>
    public static readonly string fileName = "global.conf";

    private readonly Lock fileLock = new ();
    private readonly JsonSerializerOptions _saveOptions = new JsonSerializerOptions()
    {
        WriteIndented = true
    };

    /// <summary>
    /// 保存配置到本地路径<see cref="FullPath"/>
    /// </summary>
    public void Save()
    {
        lock (fileLock) {
            try {
                using var fileStream = File.OpenWrite(FullPath);
                fileStream.Seek(0, SeekOrigin.Current);
                fileStream.SetLength(0);
                var thisText = JsonSerializer.Serialize(this, _saveOptions);
                var bytes = Encoding.UTF8.GetBytes(thisText);
                fileStream.Write(bytes);
                fileStream.Flush();
            } catch {
            }
        }
    }

    /// <summary>
    /// 使用 DeepUpdater 深拷贝当前配置，并将敏感字段置为默认值
    /// </summary>
    public GlobalConfigDto GetSafeConfig()
    {
        var copy = new GlobalConfigDto();
        _update.Update(this, copy);
        copy.LoginPassword = "";
        copy.JwtKey = "";
        copy.BlogDbConnectionString = "";
        copy.AdminMailAddress = "";

        if (copy.MailOptions == null) {
            copy.MailOptions = MailOptions.CreateDefault();
        } else {
            bool old = copy.MailOptions.UseSSL!.Value;
            copy.MailOptions = MailOptions.CreateDefault();
            copy.MailOptions.UseSSL = old;
        }

        return copy;
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    public void Update(GlobalConfigDto config)
    {
        _update.Update(config, this);
        Save();
    }

    private static readonly JsonSerializerOptions _updateOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 更新配置
    /// </summary>
    /// <param name="jsonString">此类型序列化后的结果</param>
    public bool Update(string jsonString)
    {
        try {
            var js = JsonSerializer.Deserialize<GlobalConfigDto>(jsonString, _updateOptions);
            if (js == null) return false;
            Update(js);
            return true;
        } catch(Exception e) {
            _logger.Error(e.Message + e.StackTrace);
            return false;
        }
    }
}