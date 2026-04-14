using Meilisearch;
using NanTingBlog.API.Dtos.Blogs;

namespace NanTingBlog.API.Services;

/// <summary>
/// 用于获取文章的服务
/// </summary>
public class BlogService(
    IConfiguration configuration, 
    IHttpClientFactory clientFactory, 
    MeilisearchClient meilisearch,
    ILogger logger)
{
    private readonly IConfiguration configuration = configuration;
    private readonly IHttpClientFactory clientFactory = clientFactory;
    private readonly MeilisearchClient meilisearch = meilisearch;
    private readonly ILogger logger = logger;
    private string Uid => nameof(BlogInfo).ToLower();
    public Task<IReadOnlyCollection<BlogInfo>> SearchOnContent(
        string keyword, 
        int? page = null, 
        int? limit = null)
        => Search(keyword, page, limit, [nameof(BlogInfo.Content).ToLower()]);

    public Task<IReadOnlyCollection<BlogInfo>> SearchOnName(
        string keyword,
        int? page = null,
        int? limit = null)
        => Search(keyword, page, limit, attributesToSearchOn: ["name"]);

    /// <summary>
    /// 查询文章
    /// </summary>
    /// <param name="keyword"> 查询关键字 </param>
    /// <param name="page"> 查询页码 </param>
    /// <param name="limit"> 单页数量 </param>
    /// <param name="attributesToSearchOn"> 查询字段 </param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<BlogInfo>> Search(
        string keyword = "*",
        int? page = null,
        int? limit = null,
        IEnumerable<string>? attributesToSearchOn = null)
    {
        await UpdateSetting();
        var taskInfo = meilisearch.Index(Uid);
        var query = CreateSearchQuery(limit, page);
        query.AttributesToSearchOn = attributesToSearchOn ?? [];
        try {
            var task = await taskInfo.SearchAsync<BlogInfo>(keyword, query);
            return task.Hits;
        } catch (Exception e) {
            logger.LogError($"{e.Message}\r\n{e.StackTrace}");
            return [];
        }
    }

    private async Task UpdateSetting()
    {
        var index = meilisearch.Index(Uid);
        await index.UpdateSettingsAsync(new Settings()
        {
            SearchableAttributes = [nameof(BlogInfo.Content).ToLower(), nameof(BlogInfo.Name).ToLower()],
            FilterableAttributes = [nameof(BlogInfo.Content).ToLower(), nameof(BlogInfo.Name).ToLower()]
        });
    }

    private static SearchQuery CreateSearchQuery(int? limit = null, int? page = null)
        =>  new SearchQuery()
            {
                Limit = limit,
                Page = page
            };
}
