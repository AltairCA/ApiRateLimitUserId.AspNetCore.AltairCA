using System;
using APIRateLimiterUserId.AspNetCore.AltairCA.Helpers;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Redis;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Providers.Redis
{
    public static class RedisCacheRegisterPipeLineExtension
    {
        // public static IApiRateLimiterUserIdServiceRegistration AddRedisCacheProvider(this IApiRateLimiterUserIdServiceRegistration servicef,
        //     Func<string> connectionString)
        // {
        //     var service =(ApiRateLimiterUserIdServiceRegistration) servicef;
        //     string redisConnectionString = connectionString();
        //     service.ServiceCollection.AddSingleton(typeof(IAPIRateLimiterUserIdStorageProvider), new RedisCacheProvider(
        //         new RedisManagerPool(redisConnectionString),service.Options.CachePrefix));
        //     return service;
        // }
    }
}