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
    public string BlogSaveDir { get; set { field = value; Save(); } } = Path.Combine(AppContext.BaseDirectory, "posts");

    /// <summary>
    /// 监听端口
    /// </summary>
    public List<string> Ports { get; set { field = value; Save(); } } = ["6999"];

    /// <summary>
    /// 每分钟的最大API访问次数
    /// </summary>
    public int MinuteAPIMaxVisit { get; set { field = value; Save(); } } = 50;

    /// <summary>
    /// 登录密码
    /// </summary>
    public string LoginPassword { get; set { field = value; Save(); } } = "qwertyuiopasdfghjklzxcvbnm123";

    /// <summary>
    /// jwt令牌颁发机构
    /// </summary>
    public string JwtIssuer { get; set { field = value; Save(); } } = "blogapi";

    /// <summary>
    /// jwt令牌受用人
    /// </summary>
    public string JwtAudience { get; set { field = value; Save(); } } = "blogapi";

    /// <summary>
    /// jwt私钥
    /// </summary>
    public string JwtKey { get; set { field = value; Save(); } } = "blogapiwertyu131i3o1p654d64f1g3h1j3k65641jzsxuhjcvguawsueqwrdsojga13wdwgsdr564";

    /// <summary>
    /// 数据库链接字符串
    /// </summary>
    public string BlogDbConnectionString { get; set { field = value; Save(); } } = @"Host=127.0.0.1:5432;Username=postgres;Password=123456;Database=blog";

    /// <summary>
    /// 邮箱验证服务的基本配置
    /// </summary>
    public MailOptions MailOptions { get; set { field = value; Save(); } } = new();

    /// <summary>
    /// 管理员的邮箱地址，当使用此地址注册账户时，会将你设置为Admin，以提供接口权限
    /// </summary>
    public string AdminMailAddress { get; set { field = value; Save(); } } = "";
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
    private JsonSerializerOptions options = new JsonSerializerOptions()
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
                var thisText = JsonSerializer.Serialize(this, options);
                var bytes = Encoding.UTF8.GetBytes(thisText);
                fileStream.Write(bytes);
            } catch {
            }
        }
    }
}
