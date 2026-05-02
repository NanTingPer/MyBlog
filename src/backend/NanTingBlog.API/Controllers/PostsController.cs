using Microsoft.AspNetCore.Mvc;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Services;
using NanTingBlog.API.Services.Blog;
using System.Text.Json.Serialization;
#if RELEASE
using NanTingBlog.IdentityModel.JWTIdentity;
#endif

namespace NanTingBlog.API.Controllers;

/// <summary>
/// 博文控制器
/// </summary>
[ApiController]
[Route("api/blog")]
public class PostsController(PostsService service, MarkdownService markdown) : ControllerBase
{
    private readonly PostsService service = service;
    private readonly MarkdownService markdown = markdown;

    /// <summary>
    /// 按照名称搜索
    /// </summary>
    [HttpGet("searchOnName")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchOnName([FromQuery] SearchBlogInput input)
    {
        var result = new BaseResult<List<PostInfo>>()
        {
            Data = service.QueryByName(input.KeyWord, input.Limit ?? 10, input.Page ?? 1)
        };
        return Ok(result);
    }

    /// <summary>
    /// 按照内容搜索
    /// </summary>
    [HttpGet("searchOnContent")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchOnContent([FromQuery] SearchBlogInput input)
    {
        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = service.QueryByContent(input.KeyWord, input.Limit ?? 10, input.Page ?? 1)
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取最新的文章
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> Search([FromQuery] SearchBlogInput? input)
    {
        var postResults = service.QueryByLast(5, 1);
        var simplePostResults = postResults.Select(post =>
        {
            post.Content = post.Content[0..20];
            return post;
        }).ToList();

        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = simplePostResults
        };
        return Ok(result);
    }

    /// <summary>
    /// 以页 获取文章
    /// </summary>
    [HttpGet("searchToPage")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchToPage([FromQuery] SearchBlogInput? input)
    {
        var postInfos = service.Query(input?.Limit ?? 10, input?.Page ?? 1);
        var simplePostResults = postInfos.Select(post => {
            post.Content = post.Content[0..20];
            return post;
        }).ToList();

        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = simplePostResults
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取页面数量
    /// </summary>
    [HttpGet("pageCount")]
    public async Task<ActionResult<BaseResult<int>>> PageCount([FromQuery] int limit)
    {
        return Ok(BaseResult<int>.Create(await service.CountAsync() / limit));
    }

    /// <summary>
    /// 使用Id获取文章，limit 和 page 参数将不会生效
    /// </summary>
    [HttpGet("searchOnId")]
    public async Task<ActionResult<BaseResult<PostInfo>>> SearchOnId([FromQuery] SearchBlogInput input)
    {
        var result = new BaseResult<PostInfo>()
        {
            Data = await service.QueryByKeyAsync(input.KeyWord)
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取给定标签的文章
    /// </summary>
    [HttpGet("searchOnTag")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchOnTag([FromQuery] SearchBlogInput input)
    {
        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = service.QueryByTag(input.KeyWord, input.Limit ?? 10, input.Page ?? 1)
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
        await service.DeleteByIdsAsync([.. input.Ids]);
        return Ok();
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
    public async Task<ActionResult<BaseResult<string>>> AddOrReplace([FromBody] PostInfo blog)
    {
        await service.UpdateOrAddAsync(blog);
        return Ok(new BaseResult<string>());
    }

    /// <summary>
    /// 删除全部条目
    /// </summary>
    /// <returns></returns>
#if RELEASE
    [JWT]
#endif
    [HttpPost("deleteAll")]
    public async Task<ActionResult<BaseResult<string>>> DeleteAll()
    {
        await service.DeleteAllAsync();
        return Ok(new BaseResult<string>());
    }

    /// <summary>
    /// 以页获取文章，返回文章的全部内容
    /// </summary>
#if RELEASE
    [JWT]
#endif
    [HttpGet("getAllToPage")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> GetAllToPage([FromQuery] SearchBlogInput? input)
    {
        var postInfos = service.Query(input?.Limit ?? 10, input?.Page ?? 1);
        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = postInfos
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取给定id文章的markdownToHtml
    /// </summary>
    [HttpGet("postHTML")]
    public async Task<ActionResult<BaseResult<string>>> PostHTML([FromQuery] PostHTMLInput input)
    {
        var result = new BaseResult<string>();
        if (string.IsNullOrEmpty(input.Id))
        {
            return Ok(result);
        }
        var post = await service.QueryByKeyAsync(input.Id);
        result.Data = markdown.ToHTML(post?.Content ?? "");
        return Ok(result);
    }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public class SearchBlogInput
    {
        [JsonPropertyName("keyWord"), FromQuery(Name = "keyWord")]
        public string KeyWord { get; set; } = "*";
        [JsonPropertyName("limit"), FromQuery(Name = "limit")]
        public int? Limit { get; set; } = 10;
        [JsonPropertyName("page"), FromQuery(Name = "page")]
        public int? Page { get; set; } = 1;
    }

    public class DeleteInput
    {
        [JsonPropertyName("ids"), FromQuery(Name = "ids")]
        public List<string> Ids { get; set; } = [];
    }

    public class PostHTMLInput
    {
        [JsonPropertyName("id"), FromQuery(Name = "id")]
        public string Id { get; set; } = string.Empty;
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}
