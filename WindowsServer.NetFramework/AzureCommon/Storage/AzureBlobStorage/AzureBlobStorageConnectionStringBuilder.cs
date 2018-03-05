using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Storage;

namespace WindowsServer.Azure.Storage.AzureBlobStorage
{
    public class AzureBlobStorageConnectionStringBuilder : StorageConnectionStringBuilder
    {
        public AzureBlobStorageConnectionStringBuilder()
            : this(string.Empty)
        { 
        }

        public AzureBlobStorageConnectionStringBuilder(string connectionString)
            : base()
        {
            this.ConnectionString = connectionString;
        }

        public string AccountName
        {
            get { return (string)this["AccountName"]; }
            set { this["AccountName"] = value; }
        }

        public string AccountKey
        {
            get { return (string)this["AccountKey"]; }
            set { this["AccountKey"] = value; }
        }

        public string Endpoint
        {
            get { return (string)this["Endpoint"]; }
            set { this["Endpoint"] = value; }
        }

        public string EndpointsProtocol
        {
            get { return (string)this["EndpointsProtocol"]; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this["EndpointsProtocol"] = "https";
                }
                else
                {
                    this["Endpoint"] = value;
                }
            }
        }

        public string Container 
        {
            get { return (string)this["Container"]; }
            set { this["Container"] = value; }
        }

        public override Dictionary<string, object> ParseConnectionString(string connectionString)
        {
            var items = new Dictionary<string, object>();
            items["Type"] = "AzureBlob";

            var json = JsonObject.Parse(connectionString) as JsonObject;
            items["AccountName"] = json.GetStringValue("AccountName");
            items["AccountKey"] = json.GetStringValue("AccountKey");
            items["Endpoint"] = json.GetStringValue("Endpoint");
            items["EndpointsProtocol"] = json.GetStringValue("EndpointsProtocol");
            items["Container"] = json.GetStringValue("Container");

            return items;
        }
    }
}
