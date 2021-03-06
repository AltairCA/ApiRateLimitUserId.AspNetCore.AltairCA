using APIRateLimiterUserId.AspNetCore.AltairCA.Helpers;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Providers
{
    public static class MemoryCacheProviderPipeLineExtension
    {
        public static IApiRateLimiterUserIdServiceRegistration AddMemoryCache(
            this IApiRateLimiterUserIdServiceRegistration servicef)
        {
            var service =(ApiRateLimiterUserIdServiceRegistration) servicef;
            service.ServiceCollection.AddSingleton<IAPIRateLimiterUserIdStorageProvider, MemoryCacheProvider>();
            return servicef;
        }
    }
}