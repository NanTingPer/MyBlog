using Microsoft.AspNetCore.Mvc;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Services;
using System.Text.Json.Serialization;

namespace NanTingBlog.API.Controllers;

[Route("api/blog")]
public class BlogController(BlogService service) : ControllerBase
{
    private readonly BlogService service = service;

    /// <summary>
    /// 按照名称搜索
    /// </summary>
    [HttpGet("searchOnName")]
    public async Task<ActionResult<IReadOnlyCollection<BlogInfo>>> SearchOnName(SearchBlogInput input)
    {
        return Ok(await service.SearchOnName(input.KeyWord, input.Page, input.Limit));
    }

    /// <summary>
    /// 按照内容搜索
    /// </summary>
    [HttpGet("searchOnContent")]
    public async Task<ActionResult<IReadOnlyCollection<BlogInfo>>> SearchOnContent(SearchBlogInput input)
    {
        return Ok(await service.SearchOnContent(input.KeyWord, input.Page, input.Limit));
    }

    /// <summary>
    /// 主页获取
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyCollection<BlogInfo>>> Search(SearchBlogInput input)
    {
        return Ok(await service.Search(input.KeyWord, input.Page, input.Limit));
    }

    public class SearchBlogInput
    {
        [JsonPropertyName("keyWord")]
        public string KeyWord { get; set; } = "*";
        [JsonPropertyName("limit")]
        public int? Limit { get; set; }
        [JsonPropertyName("page")]
        public int? Page { get; set; }
    }
}
