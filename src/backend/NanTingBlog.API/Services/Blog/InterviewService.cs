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
    {
        var sq = new SearchQuery()
        {
            Page = page,
            Limit = limit,
            AttributesToSearchOn = [nameof(BlogInfo.Content).ToLower()]
        };
        return Search(sq, keyword);
    }

    /// <summary>
    /// 获取包含名称的文章
    /// </summary>
    public Task<IReadOnlyCollection<BlogInfo>> SearchOnName(
        string keyword,
        int? page = null,
        int? limit = null)
    {
        var sq = new SearchQuery()
        {
            Limit = limit,
            Page = page,
            AttributesToSearchOn = [nameof(BlogInfo.Name).ToLower()]
        };
        return Search(sq, keyword);
    }

    /// <summary>
    /// 获取给定id的文章
    /// </summary>
    public async Task<BlogInfo?> SearchOnId(string id)
    {
        var sq = new SearchQuery()
        {
            Page = 1,
            Limit = 1,
            AttributesToSearchOn = [nameof(BlogInfo.Id).ToLower()],
            Filter = $"{nameof(BlogInfo.Id).ToLower()}=\"{id}\""
        };
        var values = await Search(sq, id);
        return values?.FirstOrDefault() ?? null;
    }

    /// <summary>
    /// 获取给定标签的文章
    /// </summary>
    public Task<IReadOnlyCollection<BlogInfo>> SearchOnTag(
        string keyword,
        int? page = null,
        int? limit = null)
    {
        var sq = new SearchQuery()
        {
            Page = page,
            Limit = limit,
            AttributesToSearchOn = [nameof(BlogInfo.Tag).ToLower()],
        };
        return Search(sq, keyword);
    }

    /// <summary>
    /// 查询文章
    /// </summary>
    /// <param name="keyword"> 查询关键字 </param>
    /// <param name="query"> 查询配置 </param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<BlogInfo>> Search(
        SearchQuery query,
        string keyword = "*")
    {
        await UpdateSetting();
        var taskInfo = meilisearch.Index(Uid);
        try {
            var task = await taskInfo.SearchAsync<BlogInfo>(keyword, query);
            return task.Hits ?? []; // 不然有些LINQ会阻塞
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
}
