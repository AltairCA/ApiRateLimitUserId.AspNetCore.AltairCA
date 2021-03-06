using System;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using ServiceStack;
using ServiceStack.Redis;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Providers.Redis
{
    internal class RedisCacheProvider:IAPIRateLimiterUserIdStorageProvider
    {
        private RedisManagerPool _redisManagerPool;
        private readonly string _cachePrefix;

        public RedisCacheProvider(RedisManagerPool pool,string cachePrefix)
        {
            _redisManagerPool = pool;
            _cachePrefix = cachePrefix;
        }
        public async Task<T> GetAsync<T>(string key)
        {
            using (var redis = _redisManagerPool.GetClient())
            {
                return await Task.Run(() => { return redis.Get<T>(GetPrefixedKey(key)); });
            }
        }

        public async Task SetAsync(string key, object obj, TimeSpan span)
        {
            using (var redis = _redisManagerPool.GetClient())
            {
                await Task.Run(() => {  redis.Set(GetPrefixedKey(key),obj,span); });
            }
        }

        public async Task SetAsync(string key, object obj)
        {
            using (var redis = _redisManagerPool.GetClient())
            {
                await Task.Run(() => {  redis.Set(GetPrefixedKey(key),obj); });
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (var redis = _redisManagerPool.GetClient())
            {
                await Task.Run(() => {  redis.Remove(GetPrefixedKey(key)); });
            }
        }
        private string GetPrefixedKey(string key)
        {
            return string.Concat(_cachePrefix, key);
        }
    }
}