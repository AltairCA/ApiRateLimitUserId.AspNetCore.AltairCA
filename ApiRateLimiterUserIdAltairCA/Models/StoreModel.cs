using System;
using System.Collections.Generic;

namespace APIRateLimiterUserId.AspNetCore.AltairCA.Models
{
    internal class StoreModel
    {
        public string ClientId { get; set; }
        public Dictionary<string, StoreEntries> PathEntries { get; set; } = new();

        public Dictionary<string, StoreEntries> GroupEntries { get; set; } = new();
        //public string Path { get; set; }
        //public HashSet<long> Entries { get; set; }

    }

    internal class StoreEntries
    {
        public long? LimitSetByAdmin { get; set; }
        public HashSet<long> Entries { get; set; } = new();
    }
}
