using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RedisManagerPool = ServiceStack.Redis.RedisManagerPool;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Providers.RedisWithBackup
{
    internal class RedisWithBackupStorageOptions
    {
        public string RedisConnectionString { get; set; }
        public string Prefix { get; set; }
    }

   
    internal class RedisWithBackupStorage:IAPIRateLimiterUserIdStorageProvider
    {
        private readonly IAPIRateLimiterUserIdBackupStorageProvider _backupProvider;
        
        private RedisManagerPool _redisManagerPool;
        private readonly string _cachePrefix;
        
        
        public RedisWithBackupStorage(RedisWithBackupStorageOptions options,IAPIRateLimiterUserIdBackupStorageProvider backupProvider = null)
        {
            _backupProvider = backupProvider;
            _cachePrefix = options.Prefix;
            _redisManagerPool = new RedisManagerPool(options.RedisConnectionString);
            
        }
        public async Task<T> GetAsync<T>(string key)
        {
            using (var redis = _redisManagerPool.GetClient())
            {
                var objfromred = await Task.Run(() => { return redis.Get<T>(GetPrefixedKey(key)); });
                if (objfromred == null && _backupProvider != null)
                {
                    return await _backupProvider.GetAsync<T>(GetPrefixedKey(key));
                }
                return objfromred;
            }
        }

        public async Task SetAsync(string key, object obj, TimeSpan span)
        {
            using (var redis = _redisManagerPool.GetClient())
            {
                if (_backupProvider != null)
                {
                    await _backupProvider.SetAsync(GetPrefixedKey(key),obj,span);
                }
                await Task.Run(() => {  redis.Set(GetPrefixedKey(key),obj,span); });
            }
        }

        public async Task SetAsync(string key, object obj)
        {
            using (var redis = _redisManagerPool.GetClient())
            {
                if (_backupProvider != null)
                {
                    await _backupProvider.SetAsync(GetPrefixedKey(key),obj);
                }
                await Task.Run(() => {  redis.Set(GetPrefixedKey(key),obj); });
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (var redis = _redisManagerPool.GetClient())
            {
                if (_backupProvider != null)
                {
                    await _backupProvider.RemoveAsync(GetPrefixedKey(key));
                }
                await Task.Run(() => {  redis.Remove(GetPrefixedKey(key)); });
            }
            
        }
        private string GetPrefixedKey(string key)
        {
            return string.Concat(_cachePrefix, key);
        }

        
    }
}