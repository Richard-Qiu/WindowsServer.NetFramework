using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Json;

namespace WindowsServer.Storage.LayeredStorage
{
    public class LayeredStorageConnectionStringBuilder : StorageConnectionStringBuilder
    {
        class StorageConnectionCollection
        {
            public IStorageConnection[] Connections { get; private set; }
            public StorageConnectionCollection(IStorageConnection[] connections)
            {
                Connections = connections;
            }
            public override string ToString()
            {
                var writer = new JsonWriter();
                writer.WriteStartArray();
                foreach (var connection in Connections)
                {
                    writer.WriteRawValue(connection.ConnectionString);
                }
                writer.WriteEndArray();
                return writer.ToString();
            }
        }

        public LayeredStorageConnectionStringBuilder()
            : this(string.Empty)
        {
        }

        public LayeredStorageConnectionStringBuilder(string connectionString)
            : base()
        {
            this.ConnectionString = connectionString;
        }

        public IStorageConnection[] Layers
        {
            get { return ((StorageConnectionCollection)this["Layers"]).Connections; }
            set { this["Layers"] = new StorageConnectionCollection(value); }
        }

        public override string ToString()
        {
            var writer = new JsonWriter();
            writer.WriteStartObject();
            writer.WriteRaw("Layers", ((StorageConnectionCollection)this["Layers"]).ToString());
            writer.Write("Type", "Layered");
            writer.WriteEndObject();
            return writer.ToString();
        }

        public override Dictionary<string, object> ParseConnectionString(string connectionString)
        {
            var items = new Dictionary<string, object>();

            items["Type"] = "Layered";

            var json = JsonObject.Parse(connectionString) as JsonObject;
            var connections = new List<IStorageConnection>();
            if (json.ContainsKey("Layers"))
            {
                foreach (JsonObject connectionJson in json.GetJsonArray("Layers"))
                {
                    var layeredConnectionString = connectionJson.ToString();
                    var factory = StorageProviderFactories.GetFactoryByConnectionString(layeredConnectionString);
                    var connection = factory.CreateConnection(layeredConnectionString);
                    connections.Add(connection);
                }
            }
            items["Layers"] = new StorageConnectionCollection(connections.ToArray());

            return items;
        }

    }
}
