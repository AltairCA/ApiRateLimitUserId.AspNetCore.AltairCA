using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APIRateLimiterUserId.AspNetCore.AltairCA.BackupProvider.MongoDbBackupProvider;
using APIRateLimiterUserId.AspNetCore.AltairCA.Helpers;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using APIRateLimiterUserId.AspNetCore.AltairCA.Providers;
using APIRateLimiterUserId.AspNetCore.AltairCA.Providers.Redis;
using APIRateLimiterUserId.AspNetCore.AltairCA.Providers.RedisWithBackup;
using ApiRateLimiterUserIdExample;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace ApiRateLimiterUserIdTest.Utils
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private static readonly string secretKey = "7NC9ghWvKatPLOS5NsboOeSBggKAY3eIzfH040KRpsIvImOZr9kIddzLmhSVww3L2kVBr1crHzc";
        private static readonly string issure = "AltairCA";
        private static readonly string audience = "AltairCAAudience";
        private SymmetricSecurityKey signingKey;
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
                services.AddMemoryCache();
                services.AddHttpContextAccessor();
                services.AddAPIRateLimiterUserId(options =>
                {
                    options.GlobalRateLimit = 10;
                    options.GlobalSpan = TimeSpan.FromMinutes(30);
                    options.ExcludeList = new List<string>
                    {
                        "127.0.0.1", "192.168.0.0/24"
                    };
                    options.UserIdClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
                }).AddRedisCacheProvider(() => "127.0.0.1:6379").AddMongoBackupProvider(options =>
                {
                    options.MongoCollectionName = "UserIdRecords";
                    options.MongoDbName = "UserIdLimit";
                    options.MongoConnectionString = "mongodb://localhost:27017";
                });;
                services.AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                }).AddDefaultBearerAuth(options =>
                {
                    options.Issuer = issure;
                    options.Audience = audience;
                    options.SignKey = signingKey;
                    options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
                });
            });
            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseMvc();
            });
        }
    }
}
