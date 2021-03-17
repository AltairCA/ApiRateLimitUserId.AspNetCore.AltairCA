using System;
using APIRateLimiterUserId.AspNetCore.AltairCA.Helpers;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using APIRateLimiterUserId.AspNetCore.AltairCA.Providers.Redis;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Redis;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Providers.RedisWithBackup
{
    public static class RedisWithBackupStoragePipeLineExtension
    {
        public static IApiRateLimiterUserIdServiceRegistrationBackupProvider AddRedisCacheProvider(this IApiRateLimiterUserIdServiceRegistration servicef,
            Func<string> connectionString)
        {
            var service =(ApiRateLimiterUserIdServiceRegistration) servicef;
            string redisConnectionString = connectionString();
            var options = new RedisWithBackupStorageOptions
            {
                Prefix = service.Options.CachePrefix,
                RedisConnectionString = redisConnectionString
            };
            service.ServiceCollection.AddSingleton(options);
            service.ServiceCollection.AddSingleton<IAPIRateLimiterUserIdStorageProvider, RedisWithBackupStorage>();
            return service;
        }
    }
}