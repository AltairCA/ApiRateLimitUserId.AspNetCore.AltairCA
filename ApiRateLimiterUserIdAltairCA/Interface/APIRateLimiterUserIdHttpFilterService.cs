using System;
using System.Threading.Tasks;
using APIRateLimiterUserId.AspNetCore.AltairCA.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Interface
{
    public interface APIRateLimiterUserIdHttpFilterService
    {
        Task<Tuple<bool, APIRateLimiterUserIdServiceResponse>> Validate(TimeSpan span, int limit, string groupKey);
        Task<Tuple<bool, APIRateLimiterUserIdServiceResponse>> Validate();
        void SetHeaderAndBodyIfLimitReached(ActionExecutingContext context, APIRateLimiterUserIdServiceResponse response);
        void SetHeadersIfNotLimitReached(ActionExecutedContext context, APIRateLimiterUserIdServiceResponse response);
    }
}
