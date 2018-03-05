using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Caching
{
    public class ObjectCacheContainer : BaseCacheContainer
    {
        public ObjectCache Cache
        {
            get;
            set;
        }

        public ObjectCacheContainer(ObjectCache cache = null)
        {
            this.Cache = cache == null ? MemoryCache.Default : cache;
        }

        public override object Add(string key, object value, CacheItemBaseDependency[] dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheBaseItem.PriorityLevel priority, CacheRemovedCallback onRemoveCallback)
        {
            var policy = new CacheItemPolicy();
            switch (priority)
            {
                case CacheBaseItem.PriorityLevel.Low:
                case CacheBaseItem.PriorityLevel.BelowNormal:
                case CacheBaseItem.PriorityLevel.Normal:
                case CacheBaseItem.PriorityLevel.AboveNormal:
                case CacheBaseItem.PriorityLevel.High:
                    policy.Priority = System.Runtime.Caching.CacheItemPriority.Default;
                    break;
                case CacheBaseItem.PriorityLevel.NotRemovable:
                    policy.Priority = CacheItemPriority.NotRemovable;
                    break;
                default:
                    throw new Exception("Unknown CacheBaseItem.PriorityLevel " + priority);
            }

            if (absoluteExpiration != CacheManager.NoAbsoluteExpiration)
            {
                policy.AbsoluteExpiration = absoluteExpiration;
            }
            if (slidingExpiration != CacheManager.NoSlidingExpiration)
            {
                policy.SlidingExpiration = slidingExpiration;
            }
            if (dependencies != null)
            {
                foreach (var d in dependencies)
                {
                    if (d is CacheItemFilesDependency)
                    {
                        var filesMonitor = new HostFileChangeMonitor(((CacheItemFilesDependency)d).Files);
                        policy.ChangeMonitors.Add(filesMonitor);
                    }
                    else if (d is CacheItemKeysDependency)
                    {
                        policy.ChangeMonitors.Add(this.Cache.CreateCacheEntryChangeMonitor(((CacheItemKeysDependency)d).Keys));
                    }
                }
            }

            if (onRemoveCallback != null)
            {
                policy.RemovedCallback = new CacheEntryRemovedCallback((a) =>
                {
                    CacheRemovedReason reason;
                    switch (a.RemovedReason)
                    {
                        case CacheEntryRemovedReason.Removed:
                            reason = CacheRemovedReason.Removed;
                            break;
                        case CacheEntryRemovedReason.ChangeMonitorChanged:
                            reason = CacheRemovedReason.DependencyChanged;
                            break;
                        case CacheEntryRemovedReason.Expired:
                            reason = CacheRemovedReason.Expired;
                            break;
                        case CacheEntryRemovedReason.Evicted:
                        case CacheEntryRemovedReason.CacheSpecificEviction:
                            reason = CacheRemovedReason.Underused;
                            break;
                        default:
                            throw new Exception("Unknown CacheRemovedReason " + a.RemovedReason);
                    }
                    onRemoveCallback(a.CacheItem.Key, a.CacheItem.Value, reason);
                });
            }

            return this.Cache.Add(key, value, policy);
        }

        public override object Get(string key)
        {
            return this.Cache.Get(key);
        }

        public override IEnumerator GetEnumerator()
        {
            return ((IEnumerable)this.Cache).GetEnumerator();
        }

        public override object Remove(string key)
        {
            return this.Cache.Remove(key);
        }
    }
}
