using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsServer.Caching
{
    public class CacheBaseItem
    {
        public enum PriorityLevel
        {
            Low = 1,
            BelowNormal,
            Normal,
            Default = Normal,
            AboveNormal,
            High,
            NotRemovable
        }

        public string Key
        {
            get;
            set;
        }

        public CacheItemBaseDependency[] Dependencies
        {
            get;
            set;
        }

        public DateTime AbsoluteExpiration
        {
            get;
            set;
        }

        public TimeSpan SlidingExpiration
        {
            get;
            set;
        }

        public PriorityLevel Priority
        {
            get;
            set;
        }

        public CacheRemovedCallback OnRemoveCallback
        {
            get;
            set;
        }

        public CacheBaseItem(string key)
            : this(key, null, CacheManager.NoAbsoluteExpiration, CacheManager.NoSlidingExpiration, PriorityLevel.Normal, null)
        {
        }

        public CacheBaseItem(string key, double minutesBeforeExpiration)
            : this(key, null, CacheManager.NoAbsoluteExpiration, CacheManager.NoSlidingExpiration, PriorityLevel.Normal, null)
        {
            this.AbsoluteExpiration = DateTime.Now.AddMinutes(minutesBeforeExpiration);
        }

        public CacheBaseItem(
            string key,
            CacheItemBaseDependency[] dependencies,
            DateTime absoluteExpiration,
            TimeSpan slidingExpiration,
            PriorityLevel priority,
            CacheRemovedCallback onRemoveCallback)
        {
            this.Key = key;
            this.Dependencies = dependencies;
            this.AbsoluteExpiration = absoluteExpiration;
            this.SlidingExpiration = slidingExpiration;
            this.Priority = priority;
            this.OnRemoveCallback = onRemoveCallback;
        }

        /// <summary>
        /// Sub-classes can update properties just before being added into Cache
        /// </summary>
        public virtual void OnBeingAddedIntoCache()
        {
        }

        public static string Format(string format, params object[] args)
        {
            string[] strings = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                object arg = args[i];
                if (arg is Guid)
                {
                    strings[i] = ((Guid)arg).ToString("N");
                }
                else if (arg is DateTime)
                {
                    strings[i] = ((DateTime)arg).ToString("yyyyMMddHHmmssfff");
                }
                else
                {
                    strings[i] = arg.ToString();
                }
            }
            return string.Format(format, strings);
        }


    }
}
