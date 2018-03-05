using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Json;

namespace WindowsServer.ApiProxy.Models
{
    internal class InvokeContext
    {
        public bool TraceEnabled { get; set; }
        public StringBuilder Trace { get; private set; }
        public List<InvokeItem> InvokeItems { get; set; }

        public class Format
        {
            public bool TraceEnabled { get; set; }
            public bool Trace { get; set; }
            public InvokeItem.Format InvokeItems { get; set; }
        }

        public InvokeContext()
        {
            Trace = new StringBuilder();
        }

        public static void Serialize(JsonWriter writer, InvokeContext item, Format format)
        {
            writer.WriteStartObject();

            if (format.TraceEnabled)
            {
                writer.Write("TraceEnabled", item.TraceEnabled);
            }

            if ((format.Trace) && (item.Trace != null))
            {
                writer.Write("Trace", item.Trace.ToString());
            }

            if ((format.InvokeItems != null) && (item.InvokeItems != null))
            {
                writer.WriteName("InvokeItems");
                InvokeItem.Serialize(writer, item.InvokeItems, format.InvokeItems);
            }

            writer.WriteEndObject();
        }

        public static void Serialize(JsonWriter writer, IList<InvokeContext> list, Format format)
        {
            writer.WriteStartArray();

            foreach (var item in list)
            {
                Serialize(writer, item, format);
            }

            writer.WriteEndArray();
        }

        public static InvokeContext Deserialize(JsonObject json)
        {
            var item = new InvokeContext()
            {
                TraceEnabled = json.GetBooleanValue("TraceEnabled"),
                Trace = new StringBuilder(json.GetStringValue("Trace")),
            };

            var invokeItemsJson = json.GetJsonArray("InvokeItems");
            if (invokeItemsJson != null)
            {
                item.InvokeItems = InvokeItem.Deserialize(invokeItemsJson);
            }
            return item;
        }

        public static List<InvokeContext> Deserialize(JsonArray jsonArray)
        {
            var list = new List<InvokeContext>();

            foreach (JsonObject json in jsonArray)
            {
                list.Add(Deserialize(json));
            }

            return list;
        }

        private void LogTrace(string level, string s)
        {
            if (!TraceEnabled)
            {
                return;
            }

            lock(Trace)
            {
                Trace.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                Trace.Append(" ");
                Trace.Append(level);
                Trace.Append(" ");
                Trace.AppendLine(s);
            }
        }

        public void LogTraceInfo(string s)
        {
            LogTrace("Info", s);
        }

        public void LogTraceWarn(string s)
        {
            LogTrace("Warn", s);
        }

        public void LogTraceError(string s)
        {
            LogTrace("Error", s);
        }

    }


}
