using System;
using System.Threading.Tasks;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Interface
{
    public interface IAPIRateLimiterUserIdBackupStorageProvider
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync(string key, object obj, TimeSpan span);
        Task SetAsync(string key, object obj);
        Task RemoveAsync(string key);
    }
}