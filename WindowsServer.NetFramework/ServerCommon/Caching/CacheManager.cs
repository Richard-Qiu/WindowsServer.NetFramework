using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Log;

namespace WindowsServer.Caching
{

    public static class CacheManager
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static bool _isDiagnosing = false;
        private static readonly Dictionary<string, int> _hits = new Dictionary<string, int>(2048);

        public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;
        public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;

        public static DateTime DiagnoseStartDate
        {
            get;
            private set;
        }

        public static DateTime DiagnoseStopDate
        {
            get;
            private set;
        }

        private static BaseCacheContainer Cache
        {
            get;
            set;
        }

        public static bool IsDiagnosing
        {
            get { return _isDiagnosing; }
        }

        static CacheManager()
        {
            DiagnoseStartDate = DateTime.MinValue;
            DiagnoseStopDate = DateTime.MinValue;
        }

        public static void Initialize(BaseCacheContainer cache = null)
        {
            CacheManager.Cache = (cache == null ? new ObjectCacheContainer(null) : cache);
        }

        public static void StartDiagnosing()
        {
            DiagnoseStartDate = DateTime.UtcNow;
            DiagnoseStopDate = DateTime.MinValue;
            _isDiagnosing = true;
            lock (_hits)
            {
                _hits.Clear();
            }
        }

        public static void StopDiagnosing()
        {
            if (!_isDiagnosing)
            {
                // Already stopped. just return
                return;
            }

            _isDiagnosing = false;
            DiagnoseStopDate = DateTime.UtcNow;
        }

        public static void Add(CacheBaseItem item, object value)
        {
            item.OnBeingAddedIntoCache();
            CacheManager.Cache.Add(
                item.Key,
                value,
                item.Dependencies,
                item.AbsoluteExpiration,
                item.SlidingExpiration,
                item.Priority,
                (k, v, reason) =>
                {
                    if (_isDiagnosing)
                    {
                        lock (_hits)
                        {
                            _hits.Remove(k);
                        }
                    }

                    if (item.OnRemoveCallback != null)
                    {
                        item.OnRemoveCallback.Invoke(k, v, reason);
                    }
                });
        }

        public static void Add(string key, object value)
        {
            Add(new CacheBaseItem(key), value);
        }

        public static void Add(string key, object value, double minutesBeforeExpiration)
        {
            Add(new CacheBaseItem(key, minutesBeforeExpiration), value);
        }

        public static object Remove(string key)
        {
            return CacheManager.Cache.Remove(key);
        }

        public static object Remove(CacheBaseItem item)
        {
            return Remove(item.Key);
        }

        private static bool Get<T>(string key, out T v)
        {
            var o = CacheManager.Cache.Get(key);
            if (o == null)
            {
                v = default(T);
                return false;
            }
            v = (T)o;
            return true;
        }

        public static async Task<T> GetAsync<T>(CacheBaseItem item, Func<Task<T>> buildObject)
        {
            var key = item.Key;
            T v;
            if (!Get<T>(key, out v))
            {
                v = await buildObject();
                if (v != null)
                {
                    Add(item, v);

                    if (_isDiagnosing)
                    {
                        lock (_hits)
                        {
                            _hits[key] = 0;
                        }
                    }
                }
                else
                {
                    _logger.Warn("Performance hits. Cache missed and failed to create the object for cachekey: " + key);

                    if (_isDiagnosing)
                    {
                        lock (_hits)
                        {
                            int keyValue;
                            if (_hits.TryGetValue(key, out keyValue))
                            {
                                if (keyValue >= 0)
                                {
                                    // This sure rarely happen
                                    _hits[key] = -1;
                                }
                                else
                                {
                                    _hits[key] = keyValue - 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (_isDiagnosing)
                {
                    lock (_hits)
                    {
                        int keyValue;
                        if (_hits.TryGetValue(key, out keyValue))
                        {
                            _hits[key] = keyValue + 1;
                        }
                        else
                        {
                            _hits[key] = 1;
                        }
                    }
                }
            }

            return v;
        }

        public static T Get<T>(CacheBaseItem item, Func<T> buildObject)
        {
            var key = item.Key;
            T v;
            if (!Get<T>(key, out v))
            {
                v = buildObject();
                if (v != null)
                {
                    Add(item, v);

                    if (_isDiagnosing)
                    {
                        lock (_hits)
                        {
                            _hits[key] = 0;
                        }
                    }
                }
                else
                {
                    _logger.Warn("Performance hits. Cache missed and failed to create the object for cachekey: " + key);

                    if (_isDiagnosing)
                    {
                        lock (_hits)
                        {
                            int keyValue;
                            if (_hits.TryGetValue(key, out keyValue))
                            {
                                if (keyValue >= 0)
                                {
                                    // This sure rarely happen
                                    _hits[key] = -1;
                                }
                                else
                                {
                                    _hits[key] = keyValue - 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (_isDiagnosing)
                {
                    lock (_hits)
                    {
                        int keyValue;
                        if (_hits.TryGetValue(key, out keyValue))
                        {
                            _hits[key] = keyValue + 1;
                        }
                        else
                        {
                            _hits[key] = 1;
                        }
                    }
                }
            }

            return v;
        }

        public static async Task<T> GetAsync<T>(string key, Func<Task<T>> buildObject)
        {
            return await GetAsync<T>(new CacheBaseItem(key), buildObject);
        }

        public static T Get<T>(string key, Func<T> buildObject)
        {
            return Get<T>(new CacheBaseItem(key), buildObject);
        }

        public static async Task<T> GetAsync<T>(string key, Func<Task<T>> buildObject, double minutesBeforeExpiration)
        {
            return await GetAsync<T>(new CacheBaseItem(key, minutesBeforeExpiration), buildObject);
        }

        public static T Get<T>(string key, Func<T> buildObject, double minutesBeforeExpiration)
        {
            return Get<T>(new CacheBaseItem(key, minutesBeforeExpiration), buildObject);
        }

        public static bool TryGetValue<T>(CacheBaseItem item, out T v)
        {
            var key = item.Key;
            if (!Get<T>(key, out v))
            {
                return false;
            }
            else
            {
                if (_isDiagnosing)
                {
                    lock (_hits)
                    {
                        int keyValue;
                        if (_hits.TryGetValue(key, out keyValue))
                        {
                            _hits[key] = keyValue + 1;
                        }
                        else
                        {
                            _hits[key] = 1;
                        }
                    }
                }
                return true;
            }
        }


        public static CacheHitsSnapshot GenerateHitsSnapshot()
        {
            var snapshot = new CacheHitsSnapshot()
            {
                StartDate = DiagnoseStartDate,
                StopDate = DiagnoseStopDate,
                CurrentDate = DateTime.UtcNow,
            };
            lock (_hits)
            {
                snapshot.Hits = _hits;
            }
            return snapshot;
        }

        public static IEnumerator GetEnumerator()
        {
            return CacheManager.Cache.GetEnumerator();
        }
    }
}