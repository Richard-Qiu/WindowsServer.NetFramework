using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Json;

namespace WindowsServer.ApiProxy.Models
{
    internal class InvokeRequest
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; private set; }
        public string Body { get; set; }

        public class Format
        {
            public bool Url { get; set; }
            public bool Method { get; set; }
            public bool Headers { get; set; }
            public bool Body { get; set; }
        }

        public InvokeRequest()
        {
            this.Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public static void Serialize(JsonWriter writer, InvokeRequest item, Format format)
        {
            writer.WriteStartObject();

            if (format.Url)
            {
                writer.Write("Url", item.Url);
            }

            if (format.Method)
            {
                writer.Write("Method", item.Method);
            }

            if ((format.Headers) && (item.Headers != null))
            {
                writer.WriteName("Headers");
                writer.WriteStartObject();
                foreach(var pair in item.Headers)
                {
                    writer.Write(pair.Key, pair.Value);
                }
                writer.WriteEndObject();
            }

            if (format.Body)
            {
                writer.Write("Body", item.Body);
            }

            writer.WriteEndObject();
        }

        public static void Serialize(JsonWriter writer, IList<InvokeRequest> list, Format format)
        {
            writer.WriteStartArray();

            foreach (var item in list)
            {
                Serialize(writer, item, format);
            }

            writer.WriteEndArray();
        }

        public static InvokeRequest Deserialize(JsonObject json)
        {
            JsonValue bodyJson;
            var hasBody = json.TryGetValue("Body", out bodyJson);
            var item = new InvokeRequest()
            {
                Url = json.GetStringValue("Url"),
                Method = json.GetStringValue("Method"),
                Body = hasBody ? (bodyJson.JsonType == JsonType.String ? (string)bodyJson : bodyJson.ToString()) : string.Empty,
            };

            var headersJson = json.GetJsonObject("Headers");
            if (headersJson != null)
            {
                item.Headers.Clear();
                foreach(var key in headersJson.Keys)
                {
                    item.Headers[key] = headersJson[key];
                }
            }
            return item;
        }

        public static List<InvokeRequest> Deserialize(JsonArray jsonArray)
        {
            var list = new List<InvokeRequest>();

            foreach (JsonObject json in jsonArray)
            {
                list.Add(Deserialize(json));
            }

            return list;
        }
    }


}
