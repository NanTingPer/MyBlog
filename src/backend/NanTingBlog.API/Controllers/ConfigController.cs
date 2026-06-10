using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NanTingBlog.API.Dtos;
using NanTingBlog.API.Services;
using NanTingBlog.API.Services.Identitys;

namespace NanTingBlog.API.Controllers;


/// <summary>
/// 配置控制器
/// <paramref name="service"/>
/// </summary>
[Route("config")]
public class ConfigController(GlobalConfigService service, RSAService rsaService) : ControllerBase
{
    /// <summary>
    /// 获取脱敏后的配置
    /// </summary>
    /// <returns></returns>
    [HttpGet("getConfig")]
    [Authorize(Policy = PolicyTypes.ADMIN)]
    public ActionResult<BaseResult<GlobalConfigDto>> GetConfig()
    {
        return Ok(BaseResult<GlobalConfigDto>.Create(service.GetSafeConfig()));
    }

    /// <summary>
    /// 更新配置，请将<see cref="UpdateConfigInput.Config"></see>进行RSA加密后传递
    /// </summary>
    /// <param name="gcs">全局配置对象</param>
    /// <returns></returns>
    [HttpPost("update")]
    [Authorize(Policy = PolicyTypes.ADMIN)]
    public ActionResult<BaseResult<string>> UpdateConfig([FromBody] UpdateConfigInput gcs)
    {
        var r = BaseResult<string>.Create("更新成功");
        if (gcs.Config == null || gcs.RequestId == null) {
            return Ok(BaseResult<string>.CreateError("参数错误"));
        }
        var result = rsaService.Decrypt(gcs.Config, gcs.RequestId);
        if (result == null) {
            return Ok(BaseResult<string>.CreateError("参数错误"));
        }

        if (!service.Update(result)) {
            return Ok(BaseResult<string>.CreateError("更新失败，未知原因"));
        }
        return Ok(r);
    }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public class UpdateConfigInput
    {
        /// <summary>
        /// 配置项，<see cref="GlobalConfigDto"/>Json序列化后，使用RSA加密后的字符串
        /// </summary>
        public string? Config { get; set; }

        /// <summary>
        /// RSA密钥Id，在<see cref="AuthenticationController"/>中获取
        /// </summary>
        public string? RequestId { get; set; }
    }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
}
