using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsServer.Log;
using WindowsServer.Web;

namespace WindowsServer.Azure.Cache
{
    //public class AzureCacheContainer : BaseCacheContainer
    //{
    //    private static Logger _logger = LogManager.GetCurrentClassLogger();

    //    private DataCache Cache
    //    {
    //        get;
    //        set;
    //    }

    //    public AzureCacheContainer(DataCache cache)
    //    {
    //        this.Cache = cache;
    //    }

    //    public override object Add(string key, object value, System.Web.Caching.CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, System.Web.Caching.CacheItemPriority priority, System.Web.Caching.CacheItemRemovedCallback onRemoveCallback)
    //    {
    //        _logger.Trace("Add object to azure cache. Key:" + key);

    //        if (absoluteExpiration == CacheManager.NoAbsoluteExpiration)
    //        {
    //            return this.Cache.Add(key, value);
    //        }
    //        else
    //        {
    //            var timeout = absoluteExpiration - DateTime.Now;
    //            return this.Cache.Add(key, value, timeout);
    //        }
    //    }

    //    public override object Get(string key)
    //    {
    //        return this.Cache.Get(key);
    //    }

    //    public override System.Collections.IDictionaryEnumerator GetEnumerator()
    //    {
    //        return this.GetEnumerator();
    //    }

    //    public override object Remove(string key)
    //    {
    //        return this.Remove(key);
    //    }
    //}
}
