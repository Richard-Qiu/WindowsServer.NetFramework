using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WindowsServer.Web;

namespace WindowsServer.Json
{
    public class JsonWriter
    {
        private StringBuilder _sb = null;

        public JsonWriter()
            : this(new StringBuilder(512))
        {
        }

        public JsonWriter(int capacity)
            : this(new StringBuilder(capacity))
        {
        }

        public JsonWriter(StringBuilder sb)
        {
            _sb = sb;
        }

        public override string ToString()
        {
            if (_sb[_sb.Length - 1] == ',')
            {
                return _sb.ToString(0, _sb.Length - 1);
            }
            return _sb.ToString();
        }

        public void WriteStartObject()
        {
            _sb.Append('{');
        }

        public void WriteEndObject()
        {
            if (_sb[_sb.Length - 1] == ',')
            {
                _sb.Length--;
            }
            _sb.Append("},");
        }

        public void WriteStartArray()
        {
            _sb.Append('[');
        }

        public void WriteEndArray()
        {
            if (_sb[_sb.Length - 1] == ',')
            {
                _sb.Length--;
            }
            _sb.Append("],");
        }

        public void WriteName(string name)
        {
            _sb.Append('\"');
            _sb.Append(name);
            _sb.Append("\":");
        }

        public void WriteRaw(string name, string value)
        {
            WriteName(name);
            _sb.Append(value);
            _sb.Append(',');
        }

        public void WriteJson(string name, string json)
        {
            WriteRaw(name, json);
        }

        public void WriteRawValue(string value)
        {
            _sb.Append(value);
            _sb.Append(',');
        }

        public void Write(string name, object value)
        {
            if (value is string)
            {
                Write(name, (string)value);
            }
            else if (value is Guid)
            {
                Write(name, (Guid)value);
            }
            else if (value is DateTime)
            {
                Write(name, (DateTime)value);
            }
            else if (value is Int32)
            {
                Write(name, (Int32)value);
            }
            else if (value is Int64)
            {
                Write(name, (Int64)value);
            }
            else if (value is Decimal)
            {
                Write(name, (decimal)value);
            }
            else if (value is Boolean)
            {
                Write(name, (Boolean)value);
            }
            else if (value is Double)
            {
                Write(name, (Double)value);
            }
            else if (value is Single)
            {
                Write(name, (Single)value);
            }
            else
            {
                throw new ArgumentOutOfRangeException("value", "Cannot serialize the type " + value.GetType().ToString());
            }
        }

        public void Write(string name, string value)
        {
            Write(name, value, true);
        }

        public void Write(string name, string value, bool needEncoding)
        {
            WriteName(name);
            _sb.Append('\"');
            _sb.Append(needEncoding ? HttpUtility.JavaScriptStringEncode(value) : value);
            _sb.Append("\",");
        }

        public void Write(string name, int value)
        {
            WriteName(name);
            _sb.Append(value);
            _sb.Append(',');
        }

        public void Write(string name, bool value)
        {
            WriteName(name);
            _sb.Append(value ? "true," : "false,");
        }

        public void Write(string name, long value)
        {
            WriteName(name);
            _sb.Append(value);
            _sb.Append(',');
        }
        public void Write(string name, decimal value)
        {
            WriteName(name);
            _sb.Append(value);
            _sb.Append(',');
        }

        public void Write(string name, float value)
        {
            WriteName(name);
            _sb.Append(value);
            _sb.Append(',');
        }

        public void Write(string name, double value)
        {
            WriteName(name);
            _sb.Append(value);
            _sb.Append(',');
        }

        public void Write(string name, Guid value)
        {
            WriteName(name);
            _sb.Append('\"');
            _sb.Append(value.ToString("N"));
            _sb.Append("\",");
        }

        public void Write(string name, DateTime value, bool includingMilliseconds = false)
        {
            Write(name, value, includingMilliseconds ? "yyyy-MM-dd HH:mm:ss.fff" : "yyyy-MM-dd HH:mm:ss");
        }

        public void Write(string name, DateTime value, string format)
        {
            WriteName(name);
            _sb.Append('\"');
            _sb.Append(value.ToString(format));
            _sb.Append("\",");
        }

        public void WriteValue(long value)
        {
            _sb.Append(value);
            _sb.Append(',');
        }

        public void WriteValue(DateTime value, bool includingMilliseconds = false)
        {
            _sb.Append('\"');
            _sb.Append(includingMilliseconds ? value.ToString("yyyy-MM-dd HH:mm:ss.fff") : value.ToString("yyyy-MM-dd HH:mm:ss"));
            _sb.Append('\"');
            _sb.Append(',');
        }

        public void WriteValue(string value)
        {
            _sb.Append('\"');
            _sb.Append(HttpUtility.JavaScriptStringEncode(value));
            _sb.Append("\",");
        }

        public void WriteValue(Guid value)
        {
            _sb.Append('\"');
            _sb.Append(value.ToString("N"));
            _sb.Append("\",");
        }


    }
}
