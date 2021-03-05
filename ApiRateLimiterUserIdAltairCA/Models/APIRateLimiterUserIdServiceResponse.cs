using System;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Models
{
    public class APIRateLimiterUserIdServiceResponse
    {
        public int AvaliableLimit { get; set; }
        public DateTime ResetIn { get; set; }
        public int MaxLimit { get; set; }
        public double Period { get; set; }
    }
}
