using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Json;

namespace WindowsServer.Storage
{
    public abstract class StorageConnectionStringBuilder
    {
        private Dictionary<string, object> _items = new Dictionary<string, object>();

        public string ConnectionString
        {
            get
            {
                return ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _items = ParseConnectionString("{}");
                }
                else
                {
                    _items = ParseConnectionString(value);
                }
            }
        }

        public virtual string StorageType
        {
            get { return (string)this["Type"]; }
        }

        public virtual object this[string keyword]
        {
            get { return _items[keyword]; }
            set { _items[keyword] = value; }
        }

        public void Add(string keyword, object value)
        {
            _items.Add(keyword, value);
        }

        public virtual void Clear()
        {
            _items.Clear();
        }

        public virtual bool ContainsKey(string keyword)
        {
            return _items.ContainsKey(keyword);
        }

        public virtual bool EquivalentTo(StorageConnectionStringBuilder connectionStringBuilder)
        {
            return ConnectionString.Equals(connectionStringBuilder.ConnectionString, StringComparison.InvariantCulture);
        }

        public virtual bool Remove(string keyword)
        {
            return _items.Remove(keyword);
        }

        public virtual bool TryGetValue(string keyword, out object value)
        {
            return _items.TryGetValue(keyword, out value);
        }

        public override string ToString()
        {
            var writer = new JsonWriter();
            writer.WriteStartObject();
            foreach(var pair in _items.OrderBy(i => i.Key))
            {
                writer.Write(pair.Key, pair.Value);
            }
            writer.WriteEndObject();
            return writer.ToString();
        }

        public abstract Dictionary<string, object> ParseConnectionString(string connectionString);
    }
}
