using System;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Json;
using WindowsServer.Web.Script.Serialization;

namespace WindowsServer.Json
{
    public static class JsonHelper
    {
        public static T FromJsonString<T>(string jsonString)
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return (T)jsonSerializer.ReadObject(ms);
            }
        }

        public static T FromJsonString<T>(Stream jsonStream)
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
            return (T)jsonSerializer.ReadObject(jsonStream);
        }

        public static string ToJsonString(Object obj)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        public static void OverrideJsonObject(JsonObject overridedJsonObject, JsonObject addOnJsonObject)
        {
            foreach (var item in addOnJsonObject)
            {
                overridedJsonObject[item.Key] = item.Value;
            }
        }
    }
}
