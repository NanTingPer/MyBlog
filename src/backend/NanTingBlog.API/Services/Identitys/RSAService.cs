using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace NanTingBlog.API.Services.Identitys;

/// <summary>
/// 非对称加密服务
/// </summary>
public class RSAService
{
    /// <summary> 存放RSA缓存 </summary>
    private readonly ConcurrentDictionary<string, RSA> requestCache = [];
    /// <summary> 存放RAS清除任务 </summary>
    private readonly ConcurrentDictionary<string, (Task, CancellationTokenSource)> RSACacheTask = [];
    /// <summary> 获取公钥 </summary>
    public PublicKey GetPublicKey()
    {
        var rsa = RSA.Create(2048);
        var guid = Guid.NewGuid();
        var requestId = guid.ToString("N");
        requestCache[requestId] = rsa; // 32位数字
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var cacheTask = Task.Run(async () => {
            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            if (cancellationToken.IsCancellationRequested) {
                return; // 释放取消请求，return 因为肯定是被API访问了
            }
            if (requestCache.TryRemove(requestId, out var rsa)) {
                rsa.Dispose();
            }
        }, cancellationToken);
        RSACacheTask[requestId] = (cacheTask, cancellationTokenSource);
        var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        return new PublicKey(publicKey, requestId);
    }

    /// <summary>
    /// 验证
    /// </summary>
    /// <param name="encipherValue">公钥加密后的内容</param>
    /// <param name="requestId">请求id</param>
    /// <param name="origValue">原始内容(加密前的内容，用于将解密后的内容进行比对)</param>
    /// <returns></returns>
    public bool Verify(string encipherValue, string requestId, string origValue)
    {
        try {
            if (requestId == string.Empty) return false;
            CancellationRSATask(requestId);
            if (!requestCache.TryRemove(requestId, out var rsa)) {
                return false;
            }
            var passwordBytes = Convert.FromBase64String(encipherValue);
            var decryptBytes = rsa.Decrypt(passwordBytes, RSAEncryptionPadding.Pkcs1/*OaepSHA512*/); //  jsencrypt使用pkcs1
            rsa.Dispose();
            var password = Encoding.UTF8.GetString(decryptBytes);
            if (password == origValue) {
                return true;
            }
            return false;
        } catch/*(Exception e)*/ {
            return false;
        } finally {
            CancellationRSATask(requestId);
            if (requestCache.TryRemove(requestId, out var rsa)) {
                rsa.Dispose();
            }
        }
    }

    /// <summary>
    /// 解密内容
    /// </summary>
    /// <param name="encipherValue">加密后的值</param>
    /// <param name="requestId">请求Id</param>
    /// <returns></returns>
    public string? Decrypt(string encipherValue, string requestId)
    {
        if (requestId == string.Empty) return null;
        CancellationRSATask(requestId);
        if (!requestCache.TryRemove(requestId, out var rsa)) {
            return null;
        }
        try {
            var passwordBytes = Convert.FromBase64String(encipherValue);
            var decryptBytes = rsa.Decrypt(passwordBytes, RSAEncryptionPadding.Pkcs1/*OaepSHA512*/); //  jsencrypt使用pkcs1
            rsa.Dispose();
            var password = Encoding.UTF8.GetString(decryptBytes);
            return password;
        } catch {
            return null;
        }
    }

    private void CancellationRSATask(string requestId)
    {
        if (RSACacheTask.TryGetValue(requestId, out var value)) {
            value.Item2.Cancel();
        }
    }
}

/// <summary>
/// 返回的公钥信息
/// </summary>
/// <param name="Key"> 公钥值 </param>
/// <param name="RequestId"> 请求id，使用此id以对内容进行解密 </param>
public record class PublicKey(string Key, string RequestId);