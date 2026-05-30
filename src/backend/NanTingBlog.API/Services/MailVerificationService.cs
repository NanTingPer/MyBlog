using Microsoft.Extensions.Caching.Memory;
namespace NanTingBlog.API.Services;

/// <summary>
/// 邮箱验证服务 <br />
/// 建议注册为单例服务
/// </summary>
public class MailVerificationService(IMailService service, IMemoryCache cache)
{
    private readonly Random _random = Random.Shared;

    /// <summary>
    /// 获取验证码
    /// </summary>
    public async Task<MailVerificatCodeResult> GetVerificatCode(string mailAddress, string userName)
    {
        var value = await cache.GetOrCreateAsync(mailAddress, async cacheEntry => {
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var mvd = new MailVerificatData()
            {
                Id = Guid.NewGuid().ToString(),
                Code = _random.Next(10000000, 100000000).ToString(),
            };
            await service.SendMail(new MailSendDto()
            {
                Body = $"你的验证码为: {mvd!.Code} 五分钟内有效",
                ToAddresses = [new MimeKit.MailboxAddress(userName, mailAddress)],
                Subject = "注册验证码"
            });
            return mvd;
        });
        if (value == null) {
            return await GetVerificatCode(mailAddress, userName);
        }
        return new MailVerificatCodeResult() { Id = value!.Id };
    }

    /// <summary>
    /// 验证
    /// </summary>
    /// <returns></returns>
    public bool VerificatCode(string mailAddress, string id, string code)
    {
        var mvd = cache.Get<MailVerificatData>(mailAddress);
        if (mvd == null) return false;
        if (mvd.Verificat(id, code)) {
            cache.Remove(mailAddress);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 邮箱验证数据
    /// </summary>
    private class MailVerificatData
    {
        /// <summary>
        /// 从服务获取的验证Id
        /// </summary>
        public string Id { get; set; } = "";
        /// <summary>
        /// 用户输入的验证码
        /// </summary>
        public string Code { get; set; } = "";

        /// <summary>
        /// 已验证次数
        /// </summary>
        public int VerificatCount { get; private set; } = 0;

        public event Action? VerificatTrueCallback;

        private readonly Lock _lock = new();
        private State state = State.NotVerificat;
        /// <summary>
        /// 使用给定code进行验证
        /// </summary>
        /// <returns></returns>
        public bool Verificat(string id,string userInputCode)
        {
            if (id != Id) return false;
            lock (_lock) {
                if (state == State.TrueVerificat) {
                    return false;
                }
                if (VerificatCount > 5) return false;
                VerificatCount += 1;
                if (userInputCode != Code) return false;
                if (userInputCode == Code) {
                    state = State.TrueVerificat;
                    VerificatTrueCallback?.Invoke();
                    return true;
                }
            }
            return false;
        }

        private enum State
        {
            CountMax,
            TrueVerificat,
            NotVerificat
        }
    }
}

/// <summary>
/// 请求邮箱验证服务时返回的内容
/// </summary>
public class MailVerificatCodeResult
{
    /// <summary>
    /// 此次请求的唯一验证id
    /// </summary>
    public string Id { get; set; } = "";
}