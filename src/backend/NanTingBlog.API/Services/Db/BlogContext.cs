using Microsoft.EntityFrameworkCore;
using NanTingBlog.API.Dtos.Blogs;

namespace NanTingBlog.API.Services.Db;

// -- 安装efcore cli工具
// dotnet tool install --global dotnet-ef 
// dotnet ef migrations add 初始化
// dotnet ef database update

/// <summary>
/// 博客数据库上下文
/// </summary>
public class BlogContext(GlobalConfigService gcs) : DbContext
{
    /// <summary>
    /// 博文表
    /// </summary>
    public DbSet<PostInfo> Blogs { get; set; }

    /// <summary>
    /// 友链表
    /// </summary>
    public DbSet<Friendslink> Friendslinks { get; set; }

    /// <summary>
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(gcs.BlogDbConnectionString);
    }

    /// <summary>
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostInfo>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Friendslink>()
            .HasKey(f => f.Id);
    }
}
