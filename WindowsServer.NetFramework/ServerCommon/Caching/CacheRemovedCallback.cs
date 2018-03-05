using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Caching
{
    public enum CacheRemovedReason
    {
        Removed = 1,
        Expired,
        Underused,
        DependencyChanged
    }

    public delegate void CacheRemovedCallback(string key, object value, CacheRemovedReason reason);

}
