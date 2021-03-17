using System;
using APIRateLimiterUserId.AspNetCore.AltairCA.Helpers;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.BackupProvider.MongoDbBackupProvider
{
    public static class MongoDbBackupProviderPipelineExtension
    {
        public static IApiRateLimiterUserIdServiceRegistrationBackupProvider AddMongoBackupProvider(
            this IApiRateLimiterUserIdServiceRegistrationBackupProvider servicef,Action<MongoDbBackupProviderOptions> options)
        {
            var service =(ApiRateLimiterUserIdServiceRegistration) servicef;
            MongoDbBackupProviderOptions providerOptions = new MongoDbBackupProviderOptions();
            options(providerOptions);
            service.ServiceCollection.AddSingleton(providerOptions);
            service.ServiceCollection.AddSingleton<IAPIRateLimiterUserIdBackupStorageProvider, MongoDbBackupProvider>();
            service.ServiceCollection.AddHostedService<MongoDbBackupProvider>();
            return service;
        }
    }
}