using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Json;

namespace WindowsServer.ApiProxy.Models
{
    internal class InvokeResponse
    {
        public long StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; private set; }
        public string Body { get; set; }

        public class Format
        {
            public bool StatusCode { get; set; }
            public bool Headers { get; set; }
            public bool Body { get; set; }
        }

        public InvokeResponse()
        {
            this.Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            this.Body = string.Empty;
        }


        public static void Serialize(JsonWriter writer, InvokeResponse item, Format format)
        {
            writer.WriteStartObject();

            if (format.StatusCode)
            {
                writer.Write("StatusCode", item.StatusCode);
            }

            if ((format.Headers) && (item.Headers != null))
            {
                writer.WriteName("Headers");
                writer.WriteStartObject();
                foreach (var pair in item.Headers)
                {
                    writer.Write(pair.Key, pair.Value);
                }
                writer.WriteEndObject();
            }

            if (format.Body)
            {
                var body = item.Body.Trim();
                if ((body.StartsWith("[") || body.StartsWith("{")) && (body.EndsWith("]") || body.EndsWith("}")))
                {
                    writer.WriteJson("Body", body);
                }
                else
                {
                    writer.Write("Body", item.Body);
                }
            }

            writer.WriteEndObject();
        }

        public static void Serialize(JsonWriter writer, IList<InvokeResponse> list, Format format)
        {
            writer.WriteStartArray();

            foreach (var item in list)
            {
                Serialize(writer, item, format);
            }

            writer.WriteEndArray();
        }

        public static InvokeResponse Deserialize(JsonObject json)
        {
            var item = new InvokeResponse()
            {
                StatusCode = json.GetLongValue("StatusCode"),
                Body = json.GetStringValue("Body"),
            };

            var headersJson = json.GetJsonObject("Headers");
            if (headersJson != null)
            {
                item.Headers.Clear();
                foreach (var key in headersJson.Keys)
                {
                    item.Headers[key] = headersJson[key];
                }
            }
            return item;
        }

        public static List<InvokeResponse> Deserialize(JsonArray jsonArray)
        {
            var list = new List<InvokeResponse>();

            foreach (JsonObject json in jsonArray)
            {
                list.Add(Deserialize(json));
            }

            return list;
        }
    }


}
