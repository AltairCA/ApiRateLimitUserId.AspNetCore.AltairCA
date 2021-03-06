using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Helpers;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using APIRateLimiterUserId.AspNetCore.AltairCA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Service
{
    internal class ApiRateLimiterUserIdHttpService : APIRateLimiterUserIdHttpService
    {
        private readonly IAPIRateLimiterUserIdStorageProvider _provider;
        private readonly IHttpContextAccessor _httpContext;
        private APIRateLimiterUserIdOptions _settings;

        public ApiRateLimiterUserIdHttpService(IAPIRateLimiterUserIdStorageProvider provider, IHttpContextAccessor httpContext, IOptions<APIRateLimiterUserIdOptions> settings)
        {
            _provider = provider;
            _httpContext = httpContext;
            _settings = settings.Value;
        }

        public async Task ClearLimit()
        {
            string clientId = CommonUtils.GetUserId(_settings, _httpContext);
            string path = CommonUtils.GetPath(_httpContext);
            string key = CommonUtils.GetKey(clientId);
            
            await _provider.RemoveAsync(key);
        }

        public async Task ClearLimit([Required(AllowEmptyStrings = false)] string groupKey)
        {
            string userId = CommonUtils.GetUserId(_settings, _httpContext);
            await ClearLimitForUser(userId, groupKey);
        }

        private async Task ClearLimitForUser(string userId, string groupkey)
        {
            var key = CommonUtils.GetKey(userId);
            StoreModel model = await _provider.GetAsync<StoreModel>(key);
            if (model != null)
            {
                if (model.GroupEntries != null)
                {
                    var keyOfGroup = CommonUtils.GetKey(groupkey);
                    model.GroupEntries.Remove(keyOfGroup);
                }
            }
            await _provider.SetAsync(key,model);
        }
    }
}
