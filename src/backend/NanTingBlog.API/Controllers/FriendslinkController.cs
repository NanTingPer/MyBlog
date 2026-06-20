using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NanTingBlog.API.Dtos.Blogs;
using NanTingBlog.API.Services.Blog;
using NanTingBlog.API.Services.Identitys;

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
        var allFriendlink = service.GetAll().Where(f => f.State == STATES.Passed) ;
        var result = new BaseResult<List<Friendslink>>()
        {
            Data = [.. allFriendlink]
        };
        return Ok(result);
    }

    /// <summary>
    /// 删除给定友链
    /// </summary>
    [Authorize(Policy = PolicyTypes.USER)]
    [HttpPost("delete")]
    public async Task<ActionResult<BaseResult<string>>> DeleteById([FromBody] DeleteByIdInput input)
    {
        var result = new BaseResult<string>();
        if(input.Id == null) {
            result.Code = 500;
            result.Data = "无效的id";
            return Ok(result);
        }
        return await Divide(
            admin: async userid => {
                await service.DeleteByKeyAsync(input.Id);
                return Ok(result);
            }, 
            user: async userId => {
                if (await IsArticleMine(userId, input.Id)) {
                    await service.DeleteByKeyAsync(input.Id);
                    return Ok(result);
                }
                return Ok(BaseResult<string>.CreateError("身份错误"));
            }
        );
    }

    /// <summary>
    /// <br> 添加或更新友链，如果要添加，请不要传入Id </br>
    /// <br> 无论如何都不要传入创建时间和创建字串 </br>
    /// </summary>
    [Authorize(Policy = PolicyTypes.USER)]
    [HttpPost("addOrUpdate")]
    public async Task<ActionResult<BaseResult<string>>> AddOrUpdate([FromBody] Friendslink newFriendslink)
    {
        var result = BaseResult<string>.CreateError("无效操作");
        if (newFriendslink == null) {
            return Ok(result);
        }
        return await Divide(
            admin: async userId => {
                await service.UpdateOrAddAsync(newFriendslink);
                return Ok(BaseResult<string>.Create("成功"));
            },
            user: async userId => {
                if (userId == null) {
                    return Ok(BaseResult<string>.CreateError("请先登录"));
                }
                newFriendslink.State = STATES.Pending;
                newFriendslink.UserId = userId;
                await service.UpdateOrAddAsync(newFriendslink);
                return Ok(BaseResult<string>.Create("成功"));
            }, BaseResult<string>.CreateError("身份错误")
        );
    }

    /// <summary>
    /// 根据Token用户返回数据
    /// </summary>
    /// <returns></returns>
    [HttpGet("getUserLink")]
    [Authorize(Policy = PolicyTypes.USER)]
    public async Task<ActionResult<BaseResult<List<Friendslink>>>> GetUserLink()
    {
        if (HttpContext.IsAdmin()) {
            return BaseResult<List<Friendslink>>.Create(service.GetAll());
        }

        if (HttpContext.IsUser()) {
            var uId = HttpContext.GetUserId();
            if (uId != null) {
                return Ok(BaseResult<List<Friendslink>>.Create([.. service.GetAll().Where(f => f.UserId == uId)]));
            }
        }
        return Ok(BaseResult<List<Friendslink>>.Create([]));
    }

    /// <summary>
    /// 获取状态字符串
    /// </summary>
    /// <returns></returns>
    [HttpGet("getStatuStrings")]
    [Authorize(Policy = PolicyTypes.USER)]
    public async Task<ActionResult<BaseResult<List<string>>>> GetStatuStrings()
    {
        return Ok(BaseResult<List<string>>.Create([.. Enum.GetValues<STATES>().Select(f => f.ToString())]));
    }

    private async Task<ActionResult<BaseResult<string>>> Divide(
        Func<string?, Task<ActionResult<BaseResult<string>>>> admin, 
        Func<string?, Task<ActionResult<BaseResult<string>>>> user,
        BaseResult<string>? defaultRet = null
        )
    {
        var userId = HttpContext.GetUserId();
        if (HttpContext.IsAdmin()) {
            return await admin(userId);
        } else if (HttpContext.IsUser()) {
            return await user(userId);
        }
        defaultRet ??= BaseResult<string>.CreateError("无效身份");
        return Ok(defaultRet);
    }

    /// <summary>
    /// 这个文章是此用户的吗？
    /// </summary>
    /// <returns>文章为null，用户id为null，文章用户id和用户id不相同 均返回false</returns>
    private async Task<bool> IsArticleMine(string? userId, string fid)
    {
        var targetFl = await service.QueryByKeyAsync(fid);
        if (targetFl == null) {
            return false;
        }
        if (userId == null) {
            return false;
        }
        if (userId != targetFl.UserId) {
            return false;
        }
        return true;
    }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public class DeleteByIdInput
    {
        public string? Id { get; set; }
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}
