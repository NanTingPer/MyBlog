using Microsoft.AspNetCore.Mvc;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Services.Blog;
#if RELEASE
using NanTingBlog.IdentityModel.JWTIdentity;
#endif
namespace NanTingBlog.API.Controllers;

/// <summary>
/// 友链控制器
/// </summary>
[ApiController]
[Route("api/friendlink")]
public class FriendslinkController(FriendslinkService service) : ControllerBase
{
    /// <summary>
    /// 获取全部友链
    /// </summary>
    [HttpGet("getall")]
    public ActionResult<BaseResult<List<Friendslink>>> GetAll()
    {
        var result = new BaseResult<List<Friendslink>>()
        {
            Data = service.GetAll()
        };
        return Ok(result);
    }

    /// <summary>
    /// 删除给定友链
    /// </summary>
#if RELEASE
    [JWT]
#endif
    [HttpPost("delete")]
    public async Task<ActionResult<BaseResult<string>>> DeleteById([FromBody] DeleteByIdInput input)
    {
        var result = new BaseResult<string>();
        if(input.Id == null) {
            result.Code = 500;
            result.Data = "无效的id";
            return Ok(result);
        }
        await service.DeleteByIdAsync(input.Id);
        return Ok(result);
    }

    /// <summary>
    /// <br> 添加或更新友链，如果要添加，请不要传入Id </br>
    /// <br> 无论如何都不要传入创建时间和创建字串 </br>
    /// </summary>
#if RELEASE
    [JWT]
#endif
    [HttpPost("addOrUpdate")]
    public async Task<ActionResult<BaseResult<string>>> AddOrUpdate([FromBody] Friendslink newFriendslink)
    {
        var result = new BaseResult<string>();
        if (newFriendslink == null) {
            result.Code = 500;
            result.Data = "无效的新链";
            return Ok(result);
        }
        await service.UpdateOrAddAsync(newFriendslink);
        return Ok(result);
    }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public class DeleteByIdInput
    {
        public string? Id { get; set; }
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}
