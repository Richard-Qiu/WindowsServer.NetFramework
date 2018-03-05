using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Caching
{
    public class CacheItemFilesDependency : CacheItemBaseDependency
    {
        public string[] Files { get; set; }
    }
}
