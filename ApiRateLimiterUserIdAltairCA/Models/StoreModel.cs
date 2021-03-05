using System;
using System.Collections.Generic;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Models
{
    internal class StoreModel
    {
        public string ClientId { get; set; }
        public Dictionary<string,HashSet<long>> PathEntries { get; set; }
        public Dictionary<string,HashSet<long>> GroupEntries { get; set; }
        //public string Path { get; set; }
        //public HashSet<long> Entries { get; set; }

    }
}
