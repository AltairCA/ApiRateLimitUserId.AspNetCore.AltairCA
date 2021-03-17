using System;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Models
{
    public class APIRateLimiterUserIdServiceResponse
    {
        public long AvaliableLimit { get; set; }
        public DateTime ResetIn { get; set; }
        public long MaxLimit { get; set; }
        public double Period { get; set; }
    }
}
