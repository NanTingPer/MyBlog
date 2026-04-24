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
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchOnName(SearchBlogInput input)
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
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchOnContent(SearchBlogInput input)
    {
        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = service.QueryByContent(input.KeyWord, input.Limit ?? 10, input.Page ?? 1)
        };
        return Ok(result);
    }

    /// <summary>
    /// 主页获取
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> Search(SearchBlogInput? input)
    {
        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = service.Query(input?.Limit ?? 10, input?.Page ?? 1)
        };
        return Ok(result);
    }

    /// <summary>
    /// 使用Id获取文章，limit 和 page 参数将不会生效
    /// </summary>
    [HttpGet("searchOnId")]
    public async Task<ActionResult<BaseResult<PostInfo>>> SearchOnId(SearchBlogInput input)
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
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchOnTag(SearchBlogInput input)
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
    /// 获取给定id文章的markdownToHtml
    /// </summary>
    [HttpGet("postHTML")]
    public async Task<ActionResult<BaseResult<string>>> PostHTML(PostHTMLInput input)
    {
        var result = new BaseResult<string>();
        if (string.IsNullOrEmpty(input.Id)) {
            return Ok(result);
        }
        var post = await service.QueryByKeyAsync(input.Id);
        result.Data = markdown.ToHTML(post?.Content ?? "");
        return Ok(result);
    }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public class SearchBlogInput
    {
        [JsonPropertyName("keyWord")]
        public string KeyWord { get; set; } = "*";
        [JsonPropertyName("limit")]
        public int? Limit { get; set; } = 10;
        [JsonPropertyName("page")]
        public int? Page { get; set; } = 1;
    }

    public class DeleteInput
    {
        [JsonPropertyName("ids")]
        public List<string> Ids { get; set; } = [];
    }

    public class PostHTMLInput
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}