using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Json
{
    public static class JsonValueExtensions
    {
        public static Guid GetGuidValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return Guid.Empty;
        }
        public static Guid GetGuidValue(this JsonValue self, string key, Guid defaultValue)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return defaultValue;
        }
        public static DateTime GetDateTimeValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return new DateTime(1900, 1, 1);
        }
        public static DateTime GetDateTimeValue(this JsonValue self, string key, DateTime defaultValue)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return defaultValue;
        }
        public static TimeSpan GetTimeSpanValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return new TimeSpan();
        }
        public static bool GetBooleanValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return false;
        }
        public static bool GetBooleanValue(this JsonValue self, string key, bool defaultValue)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return defaultValue;
        }
        public static char GetCharValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return '\0';
        }
        public static char GetCharValue(this JsonValue self, string key, char defaultValue)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return defaultValue;
        }
        public static short GetShortValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0;
        }
        public static short GetShortValue(this JsonValue self, string key, short defaultValue)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return defaultValue;
        }
        public static int GetIntValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0;
        }
        public static int GetIntValue(this JsonValue self, string key, int defaultValue)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return defaultValue;
        }
        public static long GetLongValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0L;
        }
        public static long GetLongValue(this JsonValue self, string key, long defaultValue)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return defaultValue;
        }
        public static ulong GetULongValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0UL;
        }
        public static decimal GetDecimalValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0;
        }
        public static float GetFloatValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0.0f;
        }
        public static string GetStringValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return string.Empty;
        }

        public static string GetStringValue(this JsonValue self, string key, string defaultValue)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return defaultValue;
        }

        public static DateTimeOffset GetDateTimeOffsetValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return new DateTimeOffset();
        }
        public static Single GetSingleValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0;
        }
        public static Double GetDoubleValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0.0D;
        }
        public static Double GetDoubleValue(this JsonValue self, string key, Double defaultValue)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return defaultValue;
        }

        public static SByte GetSByteValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0;
        }
        public static Byte GetByteValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0;
        }
        public static Uri GetUriValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return null;
        }
        public static uint GetUIntValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0;
        }
        public static ushort GetUShortValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return 0;
        }
        public static EnumType GetEnumValue<EnumType>(this JsonValue self, string key)
        {
            var v = self[key];
            if (v.JsonType == JsonType.String)
            {
                return (EnumType)Enum.Parse(typeof(EnumType), (string)v, true);
            }
            return (EnumType)Enum.ToObject(typeof(EnumType), (int)v);
        }
        public static JsonValue GetJsonValue(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key];
            }
            return null;
        }
        public static JsonObject GetJsonObject(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key] as JsonObject;
            }
            return null;
        }
        public static JsonArray GetJsonArray(this JsonValue self, string key)
        {
            if (self.ContainsKey(key))
            {
                return self[key] as JsonArray;
            }
            return null;
        }
    }
}
