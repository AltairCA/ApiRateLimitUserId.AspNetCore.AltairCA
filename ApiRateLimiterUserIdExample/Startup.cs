using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Helpers;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using APIRateLimiterUserId.AspNetCore.AltairCA.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiRateLimiterUserIdExample
{
    public class Startup
    {
        private static readonly string secretKey = "7NC9ghWvKatPLOS5NsboOeSBggKAY3eIzfH040KRpsIvImOZr9kIddzLmhSVww3L2kVBr1crHzc";
        private static readonly string issure = "AltairCA";
        private static readonly string audience = "AltairCAAudience";
        private readonly SymmetricSecurityKey signingKey;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddScoped<APIRateLimiterUserIdStorageProvider, MemoryCacheProvider>();
            services.AddAPIRateLimiterUserId(options =>
            {
                options.GlobalRateLimit = 10;
                options.GlobalSpan = TimeSpan.FromMinutes(30);
                options.ExcludeList = new List<string>
                {
                    "UserID"
                };
            });
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
    public static class Extensions{
    public static AuthenticationBuilder AddDefaultBearerAuth(
        this AuthenticationBuilder builder,
        Action<TokenProviderOptions> options)
    {
        TokenProviderOptions jwtOptions = new TokenProviderOptions();
        options(jwtOptions);
        TokenValidationParameters tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = false,
            IssuerSigningKey = jwtOptions.SignKey,
            ValidateIssuer = false,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = false,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "sub",
            RoleClaimType = "Role",
            RequireSignedTokens = false,
        };
  
        builder.AddJwtBearer((Action<JwtBearerOptions>) (c => c.TokenValidationParameters = tokenValidationParameters));
        builder.Services.Configure<TokenProviderOptions>((Action<TokenProviderOptions>) (x =>
        {
            x.Audience = jwtOptions.Audience;
            x.Expiration = jwtOptions.Expiration;
            x.Issuer = jwtOptions.Issuer;
            x.SigningCredentials = jwtOptions.SigningCredentials;
            x.SignKey = jwtOptions.SignKey;
        }));
        return builder;
    }
    }

    public class TokenProviderOptions
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public TimeSpan Expiration { get; set; } = TimeSpan.FromDays(1.0);

        public SigningCredentials SigningCredentials { get; set; }

        public SecurityKey SignKey { get; set; }
    }
}
