using System.Text;
using System.Text.Json;

namespace NanTingBlog.API.Services;

/// <summary>
/// 全局配置服务
/// </summary>
public class GlobalConfigService
{
    /// <summary> </summary>
    public GlobalConfigService()
    {
        if (!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }

        if (!File.Exists(FullPath)) {
            using (var fileStream = File.Create(FullPath)){ }
            Save();
        }
    }

    #region 配置
    /// <summary>
    /// 博文保存路径
    /// </summary>
    public string BlogSaveDir { get; set; } = Path.Combine(AppContext.BaseDirectory, "posts");
    #endregion

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
    /// <summary>
    /// 保存配置到本地路径<see cref="FullPath"/>
    /// </summary>
    public void Save()
    {
        lock (fileLock) {
            try {
                using var fileStream = File.OpenWrite(FullPath);
                var thisText = JsonSerializer.Serialize(this);
                var bytes = Encoding.UTF8.GetBytes(thisText);
                fileStream.Write(bytes);
            } catch {
            }
        }
    }
}
