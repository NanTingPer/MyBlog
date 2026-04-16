using Meilisearch;
using NanTingBlog.API;
using NanTingBlog.API.Services;
using NanTingBlog.API.Services.Blog;
using NanTingBlog.IdentityModel.JWTIdentity;
using NanTingBlog.IdentityModel.RSAIdentity;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
#region Log
builder.Services.AddLogging();
builder.Services.AddSingleton<ILogger>(services => {
    var logFactory = services.GetService<ILoggerFactory>();
    return logFactory!.CreateLogger("global");
});
#endregion

#region meilisearch HttpClient
builder.Services.AddHttpClient(HttpClientType.Meilisearch, h => {
    h.BaseAddress = new Uri("http://localhost:7700");
});
#endregion

#region MeilisearchClient
builder.Services.AddScoped<MeilisearchClient>(provider => {
    var httpClientFactory = provider.GetService<IHttpClientFactory>();
    return new MeilisearchClient(httpClientFactory!.CreateClient(HttpClientType.Meilisearch));
});
#endregion

#region GlobalConfigService
builder.Services.AddSingleton(provider => {
    if (File.Exists(GlobalConfigService.FullPath)) {
        return JsonSerializer.Deserialize<GlobalConfigService>(File.ReadAllText(GlobalConfigService.FullPath))!;
    } else {
        return new GlobalConfigService();
    }
});
#endregion

builder.Services.AddScoped<InterviewService>();
builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddOpenApi();

#if DEBUG
builder.Services.AddSwaggerGen(options => {
    var xmlPath = Path.Combine(AppContext.BaseDirectory, (Assembly.GetExecutingAssembly().GetName().Name ?? "null") + ".xml");
    if (File.Exists(xmlPath)) {
        options.IncludeXmlComments(xmlPath);
    }
});
#endif
builder.Services.AddJWTService(() => {
    return new JWTServiceOptions()
    {
        SecurityKey = () => "awdawdawdawdawdawdawdawdawdawdawdawfho",
    };
});
builder.Services.AddRSAService();
builder.Services.AddHostedService<WatchService>();

var app = builder.Build();
app.AddJWTMiddleware();
#if DEBUG
app.UseSwagger();
app.UseSwaggerUI();
#endif

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

/// <summary>
/// curl -L https://install.meilisearch.com | sh #安装meilisearch数据库
/// </summary>
public static class Note{}