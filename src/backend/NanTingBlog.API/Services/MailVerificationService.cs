using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
namespace NanTingBlog.API.Services;

/// <summary>
/// 邮箱验证服务 <br />
/// 建议注册为单例服务
/// </summary>
public class MailVerificationService(IMailService service, IMemoryCache cache)
{
    private readonly Random _random = Random.Shared;
    /// <summary>
    /// key是GUID，value是 (访问次数, 验证码)
    /// </summary>
    private readonly ConcurrentDictionary<string, (int, string)> _verificats = [];
    /// <summary>
    /// key是GUID，如果给定的GUID在字典中没有被找到，说明已经被使用。无法再次验证
    /// </summary>
    private readonly ConcurrentDictionary<string, object> _locks = [];

    /// <summary>
    /// 获取验证码
    /// </summary>
    public async Task<MailVerificatCodeResult> GetVerificatCode(string mailAddress, string userName)
    {
        var value = await cache.GetOrCreateAsync(mailAddress, async cacheEntry => {
            cacheEntry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration() { 
                EvictionCallback = (key, value, reason, state) => {
                    var id = ((MailVerificatData?)value)?.Id;
                    if (id == null) return;
                    _verificats.Remove(id, out _);
                    _locks.Remove(id, out _);
                }
            });
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var mvd = new MailVerificatData()
            {
                Id = Guid.NewGuid().ToString(),
                Code = _random.Next(10000000, 100000000).ToString(),
            };
            cacheEntry.Value = mvd;
            _verificats[mvd.Id] = (0, mvd.Code);
            await service.SendMail(new MailSendDto()
            {
                Body = $"你的验证码为: {mvd!.Code} 五分钟内有效",
                ToAddresses = [new MimeKit.MailboxAddress(userName, mailAddress)],
                Subject = "注册验证码"
            });
            return (MailVerificatData)cacheEntry.Value;
        });

        return new MailVerificatCodeResult() { Id = value!.Id };
    }

    /// <summary>
    /// 验证
    /// </summary>
    /// <returns></returns>
    public bool VerificatCode(string mailAddress, string id, string code)
    {
        var mvd = cache.Get<MailVerificatData>(mailAddress);
        if (mvd == null) {
            return false;
        }

        if (!_locks.TryGetValue(id, out object? @lock)) {
            return false; // 不存在锁了，说明被验证 并且成功了
        }
        lock(@lock) {
            if (!_verificats.TryGetValue(id, out (int count, string code) value)) {
                return false;
            }

            // 超过次数 以及验证成功，均移除锁，除非缓存过期，不然不能再次进行验证。（5分钟）
            if (value.count > 5) {
                _locks.Remove(id, out _);
                return false;
            }
            _verificats.TryUpdate(id, (value.count + 1, value.code), value);
            if (value.code == code) {
                _locks.Remove(id, out _);
                return true;
            }
            return false;
        }
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

        private readonly Lock _lock = new();
        private State state = State.NotVerificat;
        /// <summary>
        /// 使用给定code进行验证
        /// </summary>
        /// <param name="userInputCode"></param>
        /// <returns></returns>
        public bool Verificat(string userInputCode)
        {
            lock (_lock) {
                if (state == State.TrueVerificat) {
                    return false;
                }
                if (VerificatCount > 5) return false;
                VerificatCount += 1;
                if (userInputCode != Code) return false;
                if (userInputCode == Code) {
                    state = State.TrueVerificat;
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
    /// <summary>
    /// 是否发送，如果未发送 说明此邮箱有未验证的验证码
    /// </summary>
    public bool IsSend { get; set; } = false;
}

/// <summary>
/// 邮箱用户 用于表示一个需要验证的用户
/// </summary>
public record class MailUser
{
    /// <summary>
    /// 邮箱地址
    /// </summary>
    public string MailAddress { get; set; } = "";
}