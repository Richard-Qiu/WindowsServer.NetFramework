using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsServer.Json;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Json;
using System.Net;

namespace System.Web.Mvc
{
    public static class ControllerExtensions
    {
        public static string GetRequestContentString(this Controller controller)
        {
            controller.Request.InputStream.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(controller.Request.InputStream))
            {
                return reader.ReadToEnd();
            }
        }

        public static JsonValue GetRequestContentJson(this Controller controller)
        {
            return JsonValue.Parse(GetRequestContentString(controller));
        }

        public static ContentResult JsonString(this Controller controller, string json)
        {
            controller.HttpContext.Response.AddHeader("Cache-Control", "no-cache");
            return new ContentResult()
            {
                ContentEncoding = Encoding.UTF8,
                Content = json,
                ContentType = "application/json"
            };
        }

        public static ContentResult JsonResult(this Controller controller, bool succeeded)
        {
            return JsonString(
                controller,
                succeeded ? "{\"Code\":0}" : "{\"Code\":-1}");
        }

        public static ContentResult JsonResult(this Controller controller, long code, string description)
        {
            JsonWriter writer = new JsonWriter(64);
            writer.WriteStartObject();
            writer.Write("Code", code);
            writer.Write("Description", description);
            writer.WriteEndObject();

            return JsonString(controller, writer.ToString());
        }

        public static ContentResult JsonResult(this Controller controller, bool succeeded, string extraValue1Key, object extraValue1Value)
        {
            return JsonResult(controller, succeeded ? 0 : -1, string.Empty, extraValue1Key, extraValue1Value);
        }

        public static ContentResult JsonResult(this Controller controller, long code, string description, string extraValue1Key, object extraValue1Value)
        {
            Dictionary<string, object> extraValues = new Dictionary<string, object>();
            extraValues[extraValue1Key] = extraValue1Value;
            return JsonResult(controller, code, description, extraValues);
        }

        public static ContentResult JsonResult(this Controller controller, bool succeeded, Dictionary<string, object> extraValues)
        {
            return JsonResult(controller, succeeded ? 0 : -1, string.Empty, extraValues);
        }

        public static ContentResult JsonResult(this Controller controller, long code, string description, Dictionary<string, object> extraValues)
        {
            JsonWriter writer = new JsonWriter(64);
            writer.WriteStartObject();

            writer.Write("Code", code);
            if (!string.IsNullOrEmpty(description))
            {
                writer.Write("Description", description);
            }

            foreach (KeyValuePair<string, object> pair in extraValues)
            {
                writer.Write(pair.Key, pair.Value);
            }

            writer.WriteEndObject();

            return JsonString(controller, writer.ToString());
        }

        public static ActionResult StatusCodeResult(this Controller controller, HttpStatusCode statusCode, string description)
        {
            JsonWriter writer = new JsonWriter(64);
            writer.WriteStartObject();
            writer.Write("Code", 0 - (int)statusCode);
            writer.Write("Description", description);
            writer.WriteEndObject();

            return new HttpStatusCodeResult(statusCode, writer.ToString());
        }

        public static ContentResult JsonResponseJson(this Controller controller, long code, string description, string extraJson1Key, string extraJson1Value)
        {
            var extraJsons = new Dictionary<string, string>();
            extraJsons[extraJson1Key] = extraJson1Value;
            return JsonResponseJson(controller, code, description, extraJsons);
        }

        public static ContentResult JsonResponseJson(this Controller controller, long code, string extraJson1Key, string extraJson1Value)
        {
            var extraJsons = new Dictionary<string, string>();
            extraJsons[extraJson1Key] = extraJson1Value;
            return JsonResponseJson(controller, code, "Succeeded", extraJsons);
        }

        public static ContentResult JsonResponseJson(this Controller controller, long code, string description, Dictionary<string, string> extraJsons)
        {
            return JsonResponseJson(controller, code, description, extraJsons, null);
        }

        public static ContentResult JsonResponseJson(this Controller controller, long code, string description, Dictionary<string, string> extraJsons, Dictionary<string, object> extraValues)
        {
            JsonWriter writer = new JsonWriter(64);
            writer.WriteStartObject();

            writer.Write("Code", code);
            if (!string.IsNullOrEmpty(description))
            {
                writer.Write("Description", description);
            }

            if (extraJsons != null)
            {
                foreach (KeyValuePair<string, string> pair in extraJsons)
                {
                    writer.WriteRaw(pair.Key, pair.Value);
                }
            }

            if (extraValues != null)
            {
                foreach (KeyValuePair<string, object> pair in extraValues)
                {
                    writer.Write(pair.Key, pair.Value);
                }
            }

            writer.WriteEndObject();

            return JsonString(controller, writer.ToString());
        }

   }
}
