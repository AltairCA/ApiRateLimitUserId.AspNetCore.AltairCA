using System;
using System.Collections.Generic;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Models
{
    public class APIRateLimiterUserIdOptions
    {
        public string UserIdClaim { get; set; }
        public List<string> ExcludeList { get; set; }
        public int GlobalRateLimit { get; set; } = 1000;
        public TimeSpan GlobalSpan { get; set; } = TimeSpan.FromMinutes(30);
        public int StatusCode { get; set; } = 429;
        public object LimitReachedResponse = new {detail = "Quota exceeded. Maximum allowed: {0} per {1}. Please try again in {2} second(s)." };
        public string CachePrefix { get; set; } = "AltairCAAPIRATELIMIT";

    }
}
