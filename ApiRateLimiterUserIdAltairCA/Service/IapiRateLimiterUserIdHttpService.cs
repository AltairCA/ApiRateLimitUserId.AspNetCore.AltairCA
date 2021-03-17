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
    internal class IapiRateLimiterUserIdHttpService : IAPIRateLimiterUserIdHttpService
    {
        private readonly IAPIRateLimiterUserIdStorageProvider _provider;
        private readonly IHttpContextAccessor _httpContext;
        private APIRateLimiterUserIdOptions _settings;

        public IapiRateLimiterUserIdHttpService(IAPIRateLimiterUserIdStorageProvider provider, IHttpContextAccessor httpContext, IOptions<APIRateLimiterUserIdOptions> settings)
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

        public async Task ClearLimitGroup([Required(AllowEmptyStrings = false)] string groupKey)
        {
            string userId = CommonUtils.GetUserId(_settings, _httpContext);
            await ClearLimitForUserAndGroup(userId, groupKey);
        }

        public async Task ClearLimitGroup(string userId, string groupKey)
        {
            await ClearLimitForUserAndGroup(userId, groupKey);
        }

        public async Task SetLimitGroup(string userId, string group, long i)
        {
            await SetLimitForUserAndGroup(userId, group, i);
        }
        
        public async Task<long> GetRemainingLimitGroup(string userId, string group)
        {
            var key = CommonUtils.GetKey(userId);
            var model = await GetModelByUserId(userId);
            if (model == null || model.GroupEntries == null)
            {
                throw new LimitNotDefinedForGroupException("Custom Limit Is Not Defined For This User");
            }
            var keyOfGroup = CommonUtils.GetKey(group);
            model.GroupEntries.TryGetValue(keyOfGroup, out StoreEntries entry);
            if (entry == null || !entry.LimitSetByAdmin.HasValue)
                throw new LimitNotDefinedForGroupException("Custom Limit Is Not Defined For This User");
                
            return entry.LimitSetByAdmin.Value;
        }

        public async Task<long> GetCurrentCountGroup(string userId, string @group)
        {
            var key = CommonUtils.GetKey(userId);
            var model = await GetModelByUserId(userId);
            if (model == null || model.GroupEntries == null)
            {
                return 0;
            }
            var keyOfGroup = CommonUtils.GetKey(group);
            model.GroupEntries.TryGetValue(keyOfGroup, out StoreEntries entry);
            if (entry == null)
                return 0;
            return entry.Entries.Count;
        }

        private async Task SetLimitForUserAndGroup(string userId, string group, long limit)
        {
            var key = CommonUtils.GetKey(userId);
            var model = await GetModelByUserId(userId);
            if (model == null)
            {
                model = new StoreModel();
                var keyOfGroup = CommonUtils.GetKey(group);
                StoreEntries entry = new StoreEntries();
                entry.LimitSetByAdmin = limit;
                model.GroupEntries.Add(keyOfGroup,entry);
            }
            else
            {
                if (model.GroupEntries == null)
                    model.GroupEntries = new Dictionary<string, StoreEntries>();
                
                var keyOfGroup = CommonUtils.GetKey(group);
                model.GroupEntries.TryGetValue(keyOfGroup, out StoreEntries entry);
                if (entry == null)
                    entry = new StoreEntries();
                entry.LimitSetByAdmin = limit;
                model.GroupEntries.Remove(keyOfGroup);
                model.GroupEntries.Add(keyOfGroup,entry);
            }
            await _provider.SetAsync(key,model);
        }
        private async Task ClearLimitForUserAndGroup(string userId, string groupkey)
        {
            var key = CommonUtils.GetKey(userId);
            var model = await GetModelByUserId(userId);
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

        private async Task<StoreModel> GetModelByUserId(string userId)
        {
            var key = CommonUtils.GetKey(userId);
            StoreModel model = await _provider.GetAsync<StoreModel>(key);
            return model;
        }
    }
}

public class LimitNotDefinedForGroupException : Exception
{
    public LimitNotDefinedForGroupException(string message):base(message)
    {
        
    }
}
