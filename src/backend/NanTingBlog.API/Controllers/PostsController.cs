using Microsoft.AspNetCore.Mvc;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Services;
using System.Text.Json.Serialization;
using NanTingBlog.API.Services.Blog.Post;


#if RELEASE
using NanTingBlog.IdentityModel.JWTIdentity;
#endif

namespace NanTingBlog.API.Controllers;

/// <summary>
/// 博文控制器
/// </summary>
[ApiController]
[Route("api/blog")]
public class PostsController(IPostService service, MarkdownService markdown, WatchService watch) : ControllerBase
{
    private readonly IPostService service = service;
    private readonly MarkdownService markdown = markdown;

    /// <summary>
    /// 按照名称搜索
    /// </summary>
    [HttpGet("searchOnName")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchOnName([FromQuery] SearchBlogInput input)
    {
        var result = new BaseResult<List<PostInfo>>()
        {
            Data = await service.QueryByName(input.KeyWord, input.Limit ?? 10, input.Page ?? 1)
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
            Data = await service.QueryByContent(input.KeyWord, input.Limit ?? 10, input.Page ?? 1)
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取最新的文章
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> Search([FromQuery] SearchBlogInput? input)
    {
        var postResults = await service.QueryByLastNoTracking(5, 1);
        ToSimplePosts(postResults);

        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = postResults
        };
        return Ok(result);
    }

    /// <summary>
    /// 以页 获取文章
    /// </summary>
    [HttpGet("searchToPage")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchToPage([FromQuery] SearchBlogInput? input)
    {
        var postInfos = await service.QueryNoTracking(input?.Limit ?? 10, input?.Page ?? 1);
        ToSimplePosts(postInfos);

        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = postInfos
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取页面数量
    /// </summary>
    [HttpGet("pageCount")]
    public async Task<ActionResult<BaseResult<int>>> PageCount([FromQuery] int limit)
    {
        int totalCount = await service.CountAsync();
        int pages = (int)Math.Ceiling((double)totalCount / limit);
        return Ok(BaseResult<int>.Create(pages));
    }

    /// <summary>
    /// 使用Id获取文章，limit 和 page 参数将不会生效
    /// </summary>
    [HttpGet("searchOnId")]
    public async Task<ActionResult<BaseResult<PostInfo>>> SearchOnId([FromQuery] SearchBlogInput input)
    {
        var result = new BaseResult<PostInfo>()
        {
            Data = await service.QueryByKeyNoTrackingAsync(input.KeyWord)
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取给定标签的文章
    /// </summary>
    [HttpGet("searchOnTag")]
    public async Task<ActionResult<BaseResult<IReadOnlyCollection<PostInfo>>>> SearchOnTag([FromQuery] SearchBlogInput input)
    {
        var list = await service.QueryByTagNoTracking(input.KeyWord, input.Limit ?? 10, input.Page ?? 1);
        ToSimplePosts(list);
        var result = new BaseResult<IReadOnlyCollection<PostInfo>>()
        {
            Data = list
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取给定标签的数据条数，只需要传入keyWord即可，也只处理keyWord
    /// </summary>
    [HttpGet("tagCount")]
    public async Task<ActionResult<BaseResult<int>>> TagCount([FromQuery] SearchBlogInput input)
    {
        var result = new BaseResult<int>()
        {
            Data = await service.CountByCriteria(new() { Tag = input.KeyWord })
        };
        return Ok(result);
    }

    /// <summary>
    /// 获取全部的tag
    /// </summary>
    [HttpGet("tagList")]
    public async Task<ActionResult<BaseResult<int>>> TagList()
    {
        var result = new BaseResult<List<string>>()
        {
            Data = await service.Tags()
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
        foreach (var item in input.Ids) {
            await service.DeleteByKeyAsync(item);
        }
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
        var oldPost = await service.QueryByKeyNoTrackingAsync(blog.Id);
        var bodyName = blog.Name;
        var bodyContent = blog.Content;
        var oldContent = oldPost?.Content;
        var oldName = oldPost?.Name;

        var result = await service.UpdateOrAddAsync(blog);
        switch (result) {
            case UpsertResult.Add:
                await watch.Create(blog);
                break;

            case UpsertResult.Update:
                if (oldPost == null) break;
                if (oldName != bodyName) {
                    watch.Rename(oldPost.Name, bodyName);
                }
                if(oldContent != bodyContent) {
                    watch.Update(bodyName, bodyContent);
                }
                break;
        }
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
        List<PostInfo> postInfos;
        if(input?.KeyWord == null || string.IsNullOrEmpty(input.KeyWord) || input.KeyWord == "*") {
            postInfos = await service.QueryNoTracking(input?.Limit ?? 10, input?.Page ?? 1);
        } else {
            postInfos = await service.QueryByName(input.KeyWord, input?.Limit ?? 10, input?.Page ?? 1);
        }
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
        var post = await service.QueryByKeyNoTrackingAsync(input.Id);
        result.Data = markdown.ToHTML(post?.Content ?? "");
        return Ok(result);
    }

    private static void ToSimplePosts(List<PostInfo> posts)
    {
        foreach (var item in posts) {
            if (item.Content.Length <= 20) {
                item.Content = item.Content[0..item.Content.Length];
            } else {
                item.Content = item.Content[0..20];
            }
        }
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
