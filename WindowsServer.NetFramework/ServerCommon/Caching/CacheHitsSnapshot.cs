using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsServer.Caching
{
    public class CacheHitsSnapshot
    {
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public Dictionary<string, int> Hits { get; set; }
    }
}
