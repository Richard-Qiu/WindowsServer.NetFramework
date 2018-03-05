using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Json;

namespace System.Web.Http
{
    public static class ApiControllerExtensions
    {
        public static async Task<string> GetRequestContentString(this ApiController controller)
        {
            return await controller.Request.Content.ReadAsStringAsync();
        }

        public static async Task<JsonValue> GetRequestContentJson(this ApiController controller)
        {
            var json = await GetRequestContentString(controller);
            return JsonValue.Parse(json);
        }

        public static HttpResponseMessage JsonString(this ApiController controller, string json)
        {
            var response = controller.Request.CreateResponse(HttpStatusCode.OK);
            if (response.Headers.CacheControl == null)
            {
                response.Headers.CacheControl = new CacheControlHeaderValue();
            }
            response.Headers.CacheControl.NoCache = true;
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            response.Headers.Add("Access-Control-Allow-Origin", "*");

            return response;
        }

        public static HttpResponseMessage JsonResult(this ApiController controller, bool succeeded)
        {
            return JsonString(
                controller,
                succeeded ? "{\"Code\":0}" : "{\"Code\":-1}");
        }

        public static HttpResponseMessage JsonResult(this ApiController controller, long code, string description)
        {
            JsonWriter writer = new JsonWriter(64);
            writer.WriteStartObject();
            writer.Write("Code", code);
            writer.Write("Description", description);
            writer.WriteEndObject();

            return JsonString(controller, writer.ToString());
        }

        public static HttpResponseMessage JsonResult(this ApiController controller, bool succeeded, string extraValue1Key, object extraValue1Value)
        {
            return JsonResult(controller, succeeded ? 0 : -1, string.Empty, extraValue1Key, extraValue1Value);
        }

        public static HttpResponseMessage JsonResult(this ApiController controller, long code, string description, string extraValue1Key, object extraValue1Value)
        {
            Dictionary<string, object> extraValues = new Dictionary<string, object>();
            extraValues[extraValue1Key] = extraValue1Value;
            return JsonResult(controller, code, description, extraValues);
        }

        public static HttpResponseMessage JsonResult(this ApiController controller, bool succeeded, Dictionary<string, object> extraValues)
        {
            return JsonResult(controller, succeeded ? 0 : -1, string.Empty, extraValues);
        }

        public static HttpResponseMessage JsonResult(this ApiController controller, long code, string description, Dictionary<string, object> extraValues)
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

        public static HttpResponseMessage JsonResponseJson(this ApiController controller, long code, string description, string extraJson1Key, string extraJson1Value)
        {
            var extraJsons = new Dictionary<string, string>();
            extraJsons[extraJson1Key] = extraJson1Value;
            return JsonResponseJson(controller, code, description, extraJsons);
        }

        public static HttpResponseMessage JsonResponseJson(this ApiController controller, long code, string extraJson1Key, string extraJson1Value)
        {
            var extraJsons = new Dictionary<string, string>();
            extraJsons[extraJson1Key] = extraJson1Value;
            return JsonResponseJson(controller, code, "Succeeded", extraJsons);
        }

        public static HttpResponseMessage JsonResponseJson(this ApiController controller, long code, string description, Dictionary<string, string> extraJsons)
        {
            JsonWriter writer = new JsonWriter(64);
            writer.WriteStartObject();

            writer.Write("Code", code);
            if (!string.IsNullOrEmpty(description))
            {
                writer.Write("Description", description);
            }

            foreach (KeyValuePair<string, string> pair in extraJsons)
            {
                writer.WriteRaw(pair.Key, pair.Value);
            }

            writer.WriteEndObject();

            return JsonString(controller, writer.ToString());
        }

    }
}
