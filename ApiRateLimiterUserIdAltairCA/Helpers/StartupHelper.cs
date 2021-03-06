using System;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using APIRateLimiterUserId.AspNetCore.AltairCA.Models;
using APIRateLimiterUserId.AspNetCore.AltairCA.Service;
using Microsoft.Extensions.DependencyInjection;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Helpers
{
    public static class StartupHelper
    {
        public static IApiRateLimiterUserIdServiceRegistration AddAPIRateLimiterUserId(this IServiceCollection service,Action<APIRateLimiterUserIdOptions> options)
        {
            service.Configure(options);
            service.AddScoped<IAPIRateLimiterUserIdHttpFilterService, IapiRateLimiterUserIdHttpFilterService>();
            service.AddScoped<IAPIRateLimiterUserIdHttpService, IapiRateLimiterUserIdHttpService>();
            var opt = new APIRateLimiterUserIdOptions();
            options(opt);
            return new ApiRateLimiterUserIdServiceRegistration()
            {
                Options = opt,
                ServiceCollection = service
            };
        }
    }

    public interface IApiRateLimiterUserIdServiceRegistration
    {
        
    }

    internal class ApiRateLimiterUserIdServiceRegistration:IApiRateLimiterUserIdServiceRegistration,IApiRateLimiterUserIdServiceRegistrationBackupProvider
    {
        public IServiceCollection ServiceCollection { get; set; }
        public APIRateLimiterUserIdOptions Options { get; set; }
    }
    public interface IApiRateLimiterUserIdServiceRegistrationBackupProvider
    {
        
    }
}
