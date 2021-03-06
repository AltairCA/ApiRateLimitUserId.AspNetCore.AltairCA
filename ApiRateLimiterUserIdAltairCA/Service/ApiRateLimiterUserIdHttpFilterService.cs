using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Helpers;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using APIRateLimiterUserId.AspNetCore.AltairCA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Service
{
    internal class ApiRateLimiterUserIdHttpFilterService: APIRateLimiterUserIdHttpFilterService
    {
        private readonly IAPIRateLimiterUserIdStorageProvider _provider;
        private readonly IHttpContextAccessor _httpContext;
        private APIRateLimiterUserIdOptions _settings;
        public ApiRateLimiterUserIdHttpFilterService(IAPIRateLimiterUserIdStorageProvider iipRateLimiterUserIdStorageProvider, IHttpContextAccessor httpContext,IOptions<APIRateLimiterUserIdOptions> settings)
        {
            _provider = iipRateLimiterUserIdStorageProvider;
            _httpContext = httpContext;
            _settings = settings.Value;
        }

       

        public async Task<Tuple<bool, APIRateLimiterUserIdServiceResponse>> Validate()
        {
            return await Validate(_settings.GlobalSpan, _settings.GlobalRateLimit, null);
        }
        public async Task<Tuple<bool, APIRateLimiterUserIdServiceResponse>> Validate(TimeSpan span, int limit, string groupKey)
        {
            string clientId = CommonUtils.GetUserId(_settings,_httpContext);
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return new Tuple<bool, APIRateLimiterUserIdServiceResponse>(true,null);
            }

            
            if (_settings.ExcludeList != null)
            {
                foreach (string exculedIps in _settings.ExcludeList)
                {
                    if (exculedIps == clientId)
                    {
                        return new Tuple<bool, APIRateLimiterUserIdServiceResponse>(true, null);
                    }

                }
            }

            DateTime now = DateTime.UtcNow;
            string path = CommonUtils.GetPath(_httpContext);
            string key = string.Empty;
            bool isGroup = !string.IsNullOrWhiteSpace(groupKey);
            key = CommonUtils.GetKey(clientId);
            StoreModel model = await _provider.GetAsync<StoreModel>(key);
            if (model == null)
            {
                model = new StoreModel
                {
                    PathEntries = new Dictionary<string, HashSet<long>>(),
                    GroupEntries = new Dictionary<string, HashSet<long>>(),
                    ClientId = clientId
                };
            }

            var keyOfPathOrGroup = CommonUtils.GetKey(isGroup ? groupKey : path);

            long spanedOutNow = now.Ticks - span.Ticks;
            HashSet<long> Entries = new HashSet<long>();
            if (isGroup)
            {
                model.GroupEntries.TryGetValue(keyOfPathOrGroup, out Entries);
            }
            else
            {
                model.PathEntries.TryGetValue(keyOfPathOrGroup, out Entries);
            }
            if (Entries == null)
            {
                Entries = new HashSet<long>();
            }
            
            await Task.Run(() => { Entries.RemoveWhere(x => x < spanedOutNow); });
            DateTime firstDate = now.Add(span);
            if (Entries.Any())
            {
                firstDate = new DateTime(Entries.First());
                firstDate = firstDate.Add(span);
            }
            if (Entries.Count >= limit)
            {
               
                return new Tuple<bool, APIRateLimiterUserIdServiceResponse>(false,new APIRateLimiterUserIdServiceResponse{ResetIn = firstDate,MaxLimit = limit,Period = span.TotalSeconds });
            }
            Entries.Add(now.Ticks);
            if (isGroup)
            {
                model.GroupEntries.Remove(keyOfPathOrGroup);
                model.GroupEntries.Add(keyOfPathOrGroup,Entries);
            }
            else
            {
                model.PathEntries.Remove(keyOfPathOrGroup);
                model.PathEntries.Add(keyOfPathOrGroup,Entries);
            }
            
            await _provider.SetAsync(key, model);
            return new Tuple<bool, APIRateLimiterUserIdServiceResponse>(true, new APIRateLimiterUserIdServiceResponse{AvaliableLimit = limit  - Entries.Count, ResetIn = firstDate, Period = span.TotalSeconds });
        }

        public void SetHeaderAndBodyIfLimitReached(ActionExecutingContext context, APIRateLimiterUserIdServiceResponse response)
        {
            if (ShouldWriteContent(context.HttpContext, response))
            {
                var jsonString = JsonConvert.SerializeObject(_settings.LimitReachedResponse);
                jsonString = jsonString.Replace("{0}", response.MaxLimit.ToString());
                jsonString = jsonString.Replace("{1}", response.Period.ToString());
                double span = Math.Floor((response.ResetIn - DateTime.UtcNow).TotalSeconds);
                if (span < 0)
                {
                    span = 0;
                }

                jsonString = jsonString.Replace("{2}", span.ToString());
                context.Result = new ContentResult
                {
                    Content = jsonString,
                    ContentType = "application/json",
                    StatusCode = _settings.StatusCode
                };
                SetHttpHeaders(context.HttpContext, response);
            }
           
        }
        public void SetHeadersIfNotLimitReached(ActionExecutedContext context, APIRateLimiterUserIdServiceResponse response)
        {
            SetHttpHeaders(context.HttpContext, response);
        }

        private void SetHttpHeaders(HttpContext context, APIRateLimiterUserIdServiceResponse response)
        {
            if (ShouldWriteContent(context, response))
            {
                context.Response.Headers.Remove("x-rate-limit-limit");
                context.Response.Headers.Remove("x-rate-limit-remaining");
                context.Response.Headers.Remove("x-rate-limit-reset");
                context.Response.Headers.Add("x-rate-limit-limit", response.Period.ToString());
                context.Response.Headers.Add("x-rate-limit-remaining", response.AvaliableLimit.ToString());
                context.Response.Headers.Add("x-rate-limit-reset", response.ResetIn.ToString("yyyy-MM-dd HH:mm:ss \"GMT\""));
            }
        }

        private bool ShouldWriteContent(HttpContext context, APIRateLimiterUserIdServiceResponse response)
        {
            if (context.Response.Headers.ContainsKey("x-rate-limit-remaining"))
            {
                int currentlimit = Convert.ToInt32(context.Response.Headers["x-rate-limit-remaining"].ToString());
                double currentPeriod =
                    Convert.ToDouble(context.Response.Headers["x-rate-limit-limit"].ToString());
                if (currentlimit > response.AvaliableLimit || (currentlimit == response.AvaliableLimit && currentPeriod > response.Period))
                {
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}
