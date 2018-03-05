using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.DataBase
{
    public abstract class SaasDbSelector<T>
    {
        public abstract string GetConnectionString(T arg);

        public virtual Dictionary<T, List<TSource>> Group<TSource>(IEnumerable<TSource> source, Func<TSource, T> keySelector)
        {
            var groups = new Dictionary<T, List<TSource>>();
            var connectionStringMap = new Dictionary<string, T>();
            foreach(var s in source)
            {
                var key = keySelector(s);
                var connectionString = GetConnectionString(key);
                T existingKey;
                if (!connectionStringMap.TryGetValue(connectionString, out existingKey))
                {
                    connectionStringMap[connectionString] = key;
                    groups[key] = new List<TSource>();
                    existingKey = key;
                }
                groups[existingKey].Add(s);
            }
            return groups;
        }
    }
}
