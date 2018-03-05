using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsServer.Caching
{
    public abstract class BaseCacheContainer
    {
        public abstract object Add(string key, object value, CacheItemBaseDependency[] dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheBaseItem.PriorityLevel priority, CacheRemovedCallback onRemoveCallback);
        public abstract object Get(string key);
        public abstract IEnumerator GetEnumerator();
        public abstract object Remove(string key);
    }
}
