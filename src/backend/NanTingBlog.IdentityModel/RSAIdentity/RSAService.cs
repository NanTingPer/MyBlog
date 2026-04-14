using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace NanTingBlog.IdentityModel.RSAIdentity;

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
    /// <param name="origValue">原始内容</param>
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
            } else {
                return false;
            }
        } catch/*(Exception e)*/ {
            return false;
        } finally {
            CancellationRSATask(requestId);
            if (requestCache.TryRemove(requestId, out var rsa)) {
                rsa.Dispose();
            }
        }
    }
    private void CancellationRSATask(string requestId)
    {
        if (RSACacheTask.TryGetValue(requestId, out var value)) {
            value.Item2.Cancel();
        }
    }
}

public record class PublicKey(string Key, string RequestId);