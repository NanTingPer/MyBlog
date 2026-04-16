using Meilisearch;
using Microsoft.AspNetCore.Mvc;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Services.Blog;
using System.Text.Json.Serialization;

namespace NanTingBlog.API.Controllers;

/// <summary>
/// 博文控制器
/// </summary>
[Route("api/blog")]
public class BlogController(InterviewService service) : ControllerBase
{
    private readonly InterviewService service = service;

    /// <summary>
    /// 按照名称搜索
    /// </summary>
    [HttpGet("searchOnName")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<BlogInfo>>>> SearchOnName(SearchBlogInput input)
    {
        var result = new BaseResult<IReadOnlyCollection<BlogInfo>>()
        {
            Data = await service.SearchOnName(input.KeyWord, input.Page, input.Limit)
        };
        return Ok(result);
    }

    /// <summary>
    /// 按照内容搜索
    /// </summary>
    [HttpGet("searchOnContent")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<BlogInfo>>>> SearchOnContent(SearchBlogInput input)
    {
        var result = new BaseResult<IReadOnlyCollection<BlogInfo>>()
        {
            Data = await service.SearchOnContent(input.KeyWord, input.Page, input.Limit)
        };
        return Ok(result);
    }

    /// <summary>
    /// 主页获取
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<BlogInfo>>>> Search(SearchBlogInput? input)
    {
        var sq = new SearchQuery()
        {
            Limit = input?.Limit ?? 10,
            Page = input?.Page ?? 1
        };
        var result = new BaseResult<IReadOnlyCollection<BlogInfo>>()
        {
            Data = await service.Search(sq, "")
        };
        return Ok(result);
    }

    /// <summary>
    /// 使用Id获取文章，limit 和 page 参数将不会生效
    /// </summary>
    [HttpGet("searchOnId")]
    public async Task<ActionResult<BaseResult<BlogInfo>>> SearchOnId(SearchBlogInput input)
    {
        var result = new BaseResult<BlogInfo>()
        {
            Data = await service.SearchOnId(input.KeyWord)
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取给定标签的文章
    /// </summary>
    [HttpGet("searchOnTag")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<BlogInfo>>>> SearchOnTag(SearchBlogInput input)
    {
        var result = new BaseResult<IReadOnlyCollection<BlogInfo>>()
        {
            Data = await service.SearchOnTag(input.KeyWord, input.Page, input.Limit)
        };
        return Ok(result);
    }

    /// <summary>
    /// 删除给定id的文章
    /// </summary>
#if RELEASE
    [JWT]
#endif
    [HttpPost("delete")]
    public async Task<ActionResult<BaseResult<string>>> Delete([FromBody] DeleteInput input)
    {
        var ti = await service.Delete([.. input.Ids]);
        return Ok(new BaseResult<string>()
        {
            Code = GetActionResultStatu(ti)
        });
    }

    /// <summary>
    /// 添加或替换(更新)                            <br/>
    /// 如果要更新一个条目，请携带Id                <br/>
    /// 如果要创建一个条目，请不要携带Id            <br/>
    /// </summary>
#if RELEASE
    [JWT]
#endif
    [HttpPost("addOrReplace")]
    public async Task<ActionResult<BaseResult<string>>> AddOrReplace([FromBody] BlogInfo blog)
    {
        var ti = await service.AddOrReplace(blog);
        return Ok(new BaseResult<string>()
        {
            Code = GetActionResultStatu(ti)
        });
    }

    private static int GetActionResultStatu(TaskInfo taskInfo)
    {
        return taskInfo.Status switch
        {
            TaskInfoStatus.Failed => 502,
            _ => 200
        };
    }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public class SearchBlogInput
    {
        [JsonPropertyName("keyWord")]
        public string KeyWord { get; set; } = "*";
        [JsonPropertyName("limit")]
        public int? Limit { get; set; }
        [JsonPropertyName("page")]
        public int? Page { get; set; }
    }

    public class DeleteInput
    {
        [JsonPropertyName("ids")]
        public List<string> Ids { get; set; } = [];
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}