using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Json;

namespace WindowsServer.ApiProxy.Models
{
    internal class InvokeItem
    {
        private bool _completed = false;
        public bool Completed { get { return _completed; } } // 'Completed' cannot be set by JS to avoid run items infinitely. Private 'set' op cannot avoid JS assignment.

        public bool Ready { get; set; }
        public int TimeoutInSeconds { get; set; }
        public string Script { get; set; }
        public InvokeRequest Request { get; set; }
        public InvokeResponse Response { get; set; }

        public class Format
        {
            public bool Ready { get; set; }
            public bool Completed { get; set; }
            public bool TimeoutInSeconds { get; set; }
            public bool Script { get; set; }
            public InvokeRequest.Format Request { get; set; }
            public InvokeResponse.Format Response { get; set; }
        }

        public InvokeItem()
        {
            Request = new InvokeRequest();
            Response = new InvokeResponse();
        }

        public static void Serialize(JsonWriter writer, InvokeItem item, Format format)
        {
            writer.WriteStartObject();

            if (format.Ready)
            {
                writer.Write("Ready", item.Ready);
            }

            if (format.Completed)
            {
                writer.Write("Completed", item.Completed);
            }

            if (format.TimeoutInSeconds)
            {
                writer.Write("TimeoutInSeconds", item.TimeoutInSeconds);
            }

            if (format.Script)
            {
                writer.Write("Script", item.Script);
            }

            if ((format.Request != null) && (item.Request != null))
            {
                writer.WriteName("Request");
                InvokeRequest.Serialize(writer, item.Request, format.Request);
            }

            if ((format.Response != null) && (item.Response != null))
            {
                writer.WriteName("Response");
                InvokeResponse.Serialize(writer, item.Response, format.Response);
            }

            writer.WriteEndObject();
        }

        public static void Serialize(JsonWriter writer, IList<InvokeItem> list, Format format)
        {
            writer.WriteStartArray();

            foreach (var item in list)
            {
                Serialize(writer, item, format);
            }

            writer.WriteEndArray();
        }

        public static InvokeItem Deserialize(JsonObject json)
        {
            var item = new InvokeItem()
            {
                Ready = json.GetBooleanValue("Ready"),
                TimeoutInSeconds = json.GetIntValue("TimeoutInSeconds"),
                Script = json.GetStringValue("Script"),
            };

            var requestJson = json.GetJsonObject("Request");
            if (requestJson != null)
            {
                item.Request = InvokeRequest.Deserialize(requestJson);
            }
            var responseJson = json.GetJsonObject("Response");
            if (responseJson != null)
            {
                item.Response = InvokeResponse.Deserialize(responseJson);
            }
            return item;
        }

        public static List<InvokeItem> Deserialize(JsonArray jsonArray)
        {
            var list = new List<InvokeItem>();

            foreach (JsonObject json in jsonArray)
            {
                list.Add(Deserialize(json));
            }

            return list;
        }

        public void SetCompleted()
        {
            _completed = true;
        }

    }


}
