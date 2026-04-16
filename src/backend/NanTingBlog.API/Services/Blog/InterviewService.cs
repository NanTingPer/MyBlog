using Meilisearch;
using NanTingBlog.API.Dtos.Blogs;

namespace NanTingBlog.API.Services.Blog;

/// <summary>
/// 用于获取文章的服务
/// </summary>
public class InterviewService(
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

    /// <summary>
    /// 获取包含内容的文章
    /// </summary>
    public Task<IReadOnlyCollection<BlogInfo>> SearchOnContent(
        string keyword, 
        int? page = null, 
        int? limit = null)
        => Search(keyword, page, limit, [nameof(BlogInfo.Content).ToLower()]);

    /// <summary>
    /// 获取包含名称的文章
    /// </summary>
    public Task<IReadOnlyCollection<BlogInfo>> SearchOnName(
        string keyword,
        int? page = null,
        int? limit = null)
        => Search(keyword, page, limit, attributesToSearchOn: ["name"]);

    /// <summary>
    /// 获取给定id的文章
    /// </summary>
    public async Task<BlogInfo?> SearchOnId(string id)
    {
        var values = await Search(id, 1, 1, [nameof(BlogInfo.Id).ToLower()]);
        return values.FirstOrDefault();
    }

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

    /// <summary>
    /// 添加或替换文章，依据blogInfo的uuid
    /// </summary>
    public Task<TaskInfo> AddOrReplace(BlogInfo blogInfo)
    {
        var index = meilisearch.Index(Uid);
        return index.AddDocumentsAsync<BlogInfo>([blogInfo]);
    }

    /// <summary>
    /// 删除给定uid的文章
    /// </summary>
    /// <returns></returns>
    public async Task<TaskInfo> Delete(params string[] uid)
    {
        var index = meilisearch.Index(Uid);
        return await index.DeleteDocumentsAsync(uid);
    }

    private async Task UpdateSetting()
    {
        var index = meilisearch.Index(Uid);
        await index.UpdateSettingsAsync(new Settings()
        {
            SearchableAttributes = [nameof(BlogInfo.Id).ToLower(), nameof(BlogInfo.Content).ToLower(), nameof(BlogInfo.Name).ToLower()],
            FilterableAttributes = [nameof(BlogInfo.Id).ToLower(), nameof(BlogInfo.Content).ToLower(), nameof(BlogInfo.Name).ToLower()]
        });
    }

    private static SearchQuery CreateSearchQuery(int? limit = null, int? page = null)
        =>  new SearchQuery()
            {
                Limit = limit,
                Page = page
            };
}
