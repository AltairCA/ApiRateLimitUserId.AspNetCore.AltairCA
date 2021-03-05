using System;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using APIRateLimiterUserId.AspNetCore.AltairCA.Models;
using APIRateLimiterUserId.AspNetCore.AltairCA.Service;
using Microsoft.Extensions.DependencyInjection;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Helpers
{
    public static class StartupHelper
    {
        public static IServiceCollection AddAPIRateLimiterUserId(this IServiceCollection service,Action<APIRateLimiterUserIdOptions> options)
        {
            service.Configure(options);
            service.AddScoped<APIRateLimiterUserIdHttpFilterService, ApiRateLimiterUserIdHttpFilterService>();
            service.AddScoped<APIRateLimiterUserIdHttpService, ApiRateLimiterUserIdHttpService>();
            return service;
        }
    }
}
