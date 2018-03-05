using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace WindowsServer.Json
{
    [Serializable]
    public class CustomDictionaryForJson : ISerializable
    {
        public Dictionary<string, object> Dictionary = new Dictionary<string, object>();

        public CustomDictionaryForJson(SerializationInfo info, StreamingContext context)
        {
            foreach (var entry in info)
            {
                Dictionary.Add(entry.Name, entry.Value);
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (string key in Dictionary.Keys)
            {
                info.AddValue(key, Dictionary[key]);
            }
        }
    }
}
