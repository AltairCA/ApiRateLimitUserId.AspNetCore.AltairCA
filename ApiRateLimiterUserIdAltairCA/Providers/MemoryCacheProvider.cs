using System;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using APIRateLimiterUserId.AspNetCore.AltairCA.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Providers
{
    public class MemoryCacheProvider: APIRateLimiterUserIdStorageProvider
    {
        private IMemoryCache _cache;
        private APIRateLimiterUserIdOptions settings;
        public MemoryCacheProvider(IMemoryCache cache, IOptions<APIRateLimiterUserIdOptions> settings)
        {
            _cache = cache;
            this.settings = settings.Value;
        }
        
        public async Task<T> GetAsync<T>(string key)
        {
            
            T model = default;
            await Task.Run(() => { _cache.TryGetValue(GetPrefixedKey(key), out model); });

            return model;
            
        }

        public async Task SetAsync(string key, object obj, TimeSpan span)
        {
            await Task.Run(() =>
            {
                _cache.Set(GetPrefixedKey(key), obj, span);

            });
        }
        public async Task SetAsync(string key, object obj)
        {
            await Task.Run(() => { _cache.Set(GetPrefixedKey(key), obj); });
        }

        public async Task RemoveAsync(string key)
        {
            await Task.Run(() =>
            {
                _cache.Remove(GetPrefixedKey(key));
            });
        }

        private string GetPrefixedKey(string key)
        {
            return string.Concat(settings.CachePrefix, key);
        }
    }
}
