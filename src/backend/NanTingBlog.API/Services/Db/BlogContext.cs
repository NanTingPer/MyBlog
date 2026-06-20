using Microsoft.EntityFrameworkCore;
using NanTingBlog.API.Dtos;
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
    /// 用户表
    /// </summary>
    public DbSet<User> Users { get; set; }

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

        modelBuilder.Entity<Friendslink>(entity => {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => f.UserId);
            entity.Navigation(f => f.User).AutoInclude(); // 不autoinclude的话，要在查询的时候手动include，如果在查询时不需要用户信息，可以手动不include
        });

        modelBuilder.Entity<User>(entity => {
            entity.HasIndex(u => u.Name);
            entity.HasKey(u => u.Id);
            var defUser = new User()
            {
                Id = "a0000000-0000-0000-0000-000000000001",
                Name = "default",
                MailAddress = "system@localhost",
                IsBanned = true,
                CreateTime = DateTime.MinValue.Ticks
            };
            entity.HasData(defUser);
        });
    }
}
