using System;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Interface
{
    internal interface IAPIRateLimiterUserIdHttpFilterService
    {
        Task<Tuple<bool, APIRateLimiterUserIdServiceResponse>> Validate(TimeSpan span, long limit, string groupKey);
        Task<Tuple<bool, APIRateLimiterUserIdServiceResponse>> Validate();
        void SetHeaderAndBodyIfLimitReached(ActionExecutingContext context, APIRateLimiterUserIdServiceResponse response);
        void SetHeadersIfNotLimitReached(ActionExecutedContext context, APIRateLimiterUserIdServiceResponse response);
    }
}
