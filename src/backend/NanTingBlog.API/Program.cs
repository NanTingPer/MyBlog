using Markdig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using NanTingBlog.API.Dtos;
using NanTingBlog.API.Middlewares;
using NanTingBlog.API.Services;
using NanTingBlog.API.Services.Blog;
using NanTingBlog.API.Services.Db;
using NanTingBlog.API.Services.Identitys;
using NanTingBlog.API.Services.Logs;
using Serilog;
using Serilog.Formatting.Json;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var logRootDir = Path.Combine(AppContext.BaseDirectory, "logs");
builder.Services.AddSerilog(lc => 
    lc      
            .WriteTo.Console()
            .WriteTo.File(path: Path.Combine(logRootDir, "log_asp_json_formatter.log"), formatter: new JsonFormatter(), rollingInterval: RollingInterval.Month, retainedFileCountLimit: 20)
            .WriteTo.File(path: Path.Combine(logRootDir, "log_asp.log"), outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Month, retainedFileCountLimit: 20));

builder.Services.AddSingleton<LoginLogger>();
builder.Services.AddSingleton<ServiceLogger>();

var markdown = new MarkdownPipelineBuilder();
var markdownPipeline = markdown
    .UseAdvancedExtensions() // 高级扩展
    .UseCitations() // 正文引用
    .UseDefinitionLists() // dl dt dd列表
    .UseMathematics()
    .UseYamlFrontMatter()
    .Build()
    ; // 内联数学

builder.Services.AddSingleton<MarkdownPipeline>(op => markdownPipeline);
builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<UserPasswordHasher>();

#region DbContext Scoped
builder.Services.AddDbContext<BlogContext>(ServiceLifetime.Scoped);
#endregion

#region GlobalConfigService
builder.Services.AddSingleton<GlobalConfigService>(provider => {
    var log_service = provider.GetService<ServiceLogger>()!;
    var service = new GlobalConfigService(log_service);

    if (File.Exists(GlobalConfigService.FullPath)) {
        var gcd = JsonSerializer.Deserialize<GlobalConfigDto>(File.ReadAllText(GlobalConfigService.FullPath))!;
        service.Update(gcd);
        return service;
    } else {
        service.Update(GlobalConfigDto.CreateDefault());
        return service;
    }
});
#endregion

builder.Services.AddScoped<PostsService>(); // 博文服务
builder.Services.AddScoped<FriendslinkService>(); // 友链服务
builder.Services.AddSingleton<WatchService>();// 文章服务依赖此服务
builder.Services.AddHostedService(services => services.GetService<WatchService>()!);
builder.Services.AddScoped<UserService>(); // 用户服务

#region 邮箱验证服务 依赖MSCache、GlobaleConfigService
builder.Services.AddSingleton<IMailService, MailService>(provider => {
    var config = provider.GetService<GlobalConfigService>();
    var mailService = new MailService(() => config!.MailOptions);
    return mailService;
});
builder.Services.AddSingleton<MailVerificationService>();
#endregion

builder.Services.AddSingleton<JwtService>();
builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddOpenApi();

#region swagger
#if DEBUG
builder.Services.AddSwaggerGen(options => {
    var xmlPath = Path.Combine(AppContext.BaseDirectory, (System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "null") + ".xml");
    if (File.Exists(xmlPath)) {
        options.IncludeXmlComments(xmlPath);
    }
});
#endif
#endregion

#region JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();// Bearer
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IServiceProvider>((opt, provider) => {
        var jwtService = provider.GetService<JwtService>();
        opt.TokenValidationParameters = jwtService!.JwtValidationParameters;
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PolicyTypes.ADMIN, policy =>
        policy.AddRequirements(Policy.AdminPolicyRequirement))
    .AddPolicy(PolicyTypes.USER, policy =>
        policy.AddRequirements(Policy.UserPolicyRequirement))
    ;
builder.Services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();
builder.Services.AddAuthorization();
#endregion

#region CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin();
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });
});
#endregion

builder.Services.AddSingleton<RSAService>();

#region SpeedMiddleware And Hosted
builder.Services.AddSingleton<SpeedMiddleware>();
builder.Services.AddHostedService(provider => provider.GetService<SpeedMiddleware>()!);
#endregion

var app = builder.Build();
app.UseMiddleware<SpeedMiddleware>();
app.UseCors("AllowAll");
#if DEBUG
app.UseSwagger();
app.UseSwaggerUI();
#endif

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}
var gcs = app.Services.GetService<GlobalConfigService>();
var ports = gcs!.Ports;
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();  //先认证 再授权 
app.MapControllers();
app.Urls.Clear();
foreach (var port in ports) {
    string url = $"http://127.0.0.1:{port}";
    app.Urls.Add(url);
}
app.Run();

/// <summary>
/// curl -L https://install.meilisearch.com | sh #安装meilisearch数据库
/// </summary>
public static class Note{}