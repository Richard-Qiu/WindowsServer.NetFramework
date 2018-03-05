using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage.LocalFileStorage
{
    public class LocalFileStorageConnectionStringBuilder : StorageConnectionStringBuilder
    {
        public LocalFileStorageConnectionStringBuilder()
            : this(string.Empty)
        {
        }

        public LocalFileStorageConnectionStringBuilder(string connectionString)
            : base()
        {
            this.ConnectionString = connectionString;
        }

        public string RootPath
        {
            get { return (string)this["RootPath"]; }
            set { this["RootPath"] = value; }
        }

        public string RootContainer
        {
            get { return (string)this["RootContainer"]; }
            set { this["RootContainer"] = value; }
        }

        public override Dictionary<string, object> ParseConnectionString(string connectionString)
        {
            var items = new Dictionary<string, object>();

            items["Type"] = "LocalFile";

            var json = JsonObject.Parse(connectionString) as JsonObject;
            items["RootPath"] = json.GetStringValue("RootPath");
            items["RootContainer"] = json.GetStringValue("RootContainer");

            return items;
        }
    }
}
