using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using WindowsServer.Caching;

namespace WindowsServer.Web.Caching
{
    public class AspNetCacheContainer : BaseCacheContainer
    {
        private Cache Cache
        {
            get;
            set;
        }

        public AspNetCacheContainer(Cache cache)
        {
            this.Cache = cache;
        }

        public override object Add(string key, object value, CacheItemBaseDependency[] dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheBaseItem.PriorityLevel priority, CacheRemovedCallback onRemoveCallback)
        {
            CacheItemPriority aspnetPriority;
            switch(priority)
            {
                case CacheBaseItem.PriorityLevel.Low:
                    aspnetPriority = CacheItemPriority.Low;
                    break;
                case CacheBaseItem.PriorityLevel.BelowNormal:
                    aspnetPriority = CacheItemPriority.BelowNormal;
                    break;
                case CacheBaseItem.PriorityLevel.Normal:
                    aspnetPriority = CacheItemPriority.Normal;
                    break;
                case CacheBaseItem.PriorityLevel.AboveNormal:
                    aspnetPriority = CacheItemPriority.AboveNormal;
                    break;
                case CacheBaseItem.PriorityLevel.High:
                    aspnetPriority = CacheItemPriority.High;
                    break;
                case CacheBaseItem.PriorityLevel.NotRemovable:
                    aspnetPriority = CacheItemPriority.NotRemovable;
                    break;
                default:
                    throw new Exception("Unknown CacheBaseItem.PriorityLevel " + priority);
            }

            CacheDependency dependency = null;
            if (dependencies != null)
            {
                foreach(var d in dependencies)
                {
                    if (d is CacheItemFilesDependency)
                    {
                        dependency = new CacheDependency(((CacheItemFilesDependency)d).Files);
                    }
                    else if (d is CacheItemKeysDependency)
                    {
                        dependency = new CacheDependency(null, ((CacheItemKeysDependency)d).Keys);
                    }
                }
            }

            return this.Cache.Add(
                key,
                value,
                dependency,
                absoluteExpiration,
                slidingExpiration,
                aspnetPriority,
                onRemoveCallback == null ? null : new CacheItemRemovedCallback((k, v, r) =>
                    {
                        CacheRemovedReason reason;
                        switch(r)
                        {
                            case CacheItemRemovedReason.Removed:
                                reason = CacheRemovedReason.Removed;
                                break;
                            case CacheItemRemovedReason.DependencyChanged:
                                reason = CacheRemovedReason.DependencyChanged;
                                break;
                            case CacheItemRemovedReason.Expired:
                                reason = CacheRemovedReason.Expired;
                                break;
                            case CacheItemRemovedReason.Underused:
                                reason = CacheRemovedReason.Underused;
                                break;
                            default:
                                throw new Exception("Unknown CacheRemovedReason " + r);
                        }
                        onRemoveCallback(k, v, reason);
                    })
            );
        }

        public override object Get(string key)
        {
            return this.Cache.Get(key);
        }

        public override IEnumerator GetEnumerator()
        {
            return this.Cache.GetEnumerator();
        }

        public override object Remove(string key)
        {
            return this.Cache.Remove(key);
        }

    }
}
