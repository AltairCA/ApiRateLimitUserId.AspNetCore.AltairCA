using System;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Interface;
using APIRateLimiterUserId.AspNetCore.AltairCA.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace APIRateLimiterUserId.AspNetCore.AltairCA
{
    public class APIRateLimiterUserIdHttpAttribute: Attribute , IAsyncActionFilter
    {
        private TimeSpan _span;
        private int _limit;
        private string _groupKey;
        public APIRateLimiterUserIdHttpAttribute()
        {
            _span = TimeSpan.MinValue;
            _limit = int.MinValue;
            _groupKey = null;
        }
        public APIRateLimiterUserIdHttpAttribute(double seconds,int limit,string groupkey = null)
        {

            _span = TimeSpan.FromSeconds(seconds);
            _limit = limit;
            _groupKey = groupkey;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            APIRateLimiterUserIdHttpFilterService service = context.HttpContext.RequestServices.GetService<APIRateLimiterUserIdHttpFilterService>();
            var settings = context.HttpContext.RequestServices.GetService<IOptions<APIRateLimiterUserIdOptions>>()
                .Value;
            Tuple<bool, APIRateLimiterUserIdServiceResponse> res = null;
            if (_span == TimeSpan.MinValue)
            {
                _span = settings.GlobalSpan;
            }

            if (_limit == int.MinValue)
            {
                _limit = settings.GlobalRateLimit;
            }
            res= await service.Validate(_span,_limit,_groupKey);
            if (res.Item1)
            {
                var resultContext = await next();
                if (res.Item2 != null)
                {
                    service.SetHeadersIfNotLimitReached(resultContext,res.Item2);
                }
            }
            else
            {
                
                service.SetHeaderAndBodyIfLimitReached(context,res.Item2);
            }

        }
    }
}
