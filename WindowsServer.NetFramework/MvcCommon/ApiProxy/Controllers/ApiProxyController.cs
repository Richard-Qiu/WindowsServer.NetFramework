using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WindowsServer.ApiProxy.Models;
using WindowsServer.Jint;
using WindowsServer.Json;
using WindowsServer.Web;

namespace WindowsServer.ApiProxy.Controllers
{
    public class ApiProxyController : Controller
    {
        private class JSLogger
        {
            private InvokeContext _context;
            public JSLogger(InvokeContext context)
            {
                _context = context;
            }
            public void Info(string s)
            {
                _context.LogTraceInfo(s);
            }
            public void Warn(string s)
            {
                _context.LogTraceWarn(s);
            }
            public void Error(string s)
            {
                _context.LogTraceError(s);
            }
        }

        public static bool Enabled { get; set; }

        //[HttpPost]
        public async Task<ActionResult> BatchInvoke()
        {
            if (!Enabled)
            {
                return this.JsonResult(-1, "ApiProxy is not enabled.");
            }

            InvokeContext context;
            if (HttpContext.Request.HttpMethod == "GET")
            {
                context = new InvokeContext()
                {
                    TraceEnabled = true,
                    InvokeItems = new List<InvokeItem>(),
                };
                var item0 = new InvokeItem()
                {
                    Ready = true,
                    Request = new InvokeRequest()
                    {
                        Url = "http://www.baidu.com",
                        Method = "POST",
                        Body = "abc",
                    },
                    Response = new InvokeResponse()
                    {
                    },
                    Script = "function postprocess(context, index, item) { if (item.Response.StatusCode==200) context.InvokeItems[1].Ready = true; }",
                };
                var item1 = new InvokeItem()
                {
                    Request = new InvokeRequest()
                    {
                        Url = "http://www.yipinapp.com",
                    },
                    Response = new InvokeResponse()
                    {
                    },
                    Script = "function preprocess(context, index, item) { logger.Info('preprocess'); item.Response.StatusCode=500; }\r\n function postprocess(context, index, item) { item.Response.Body = \"abc\"; item.Completed = true; logger.Error('postprocess'); }",
                };
                context.InvokeItems.Add(item0);
                context.InvokeItems.Add(item1);
            }
            else
            {
                var json = this.GetRequestContentJson() as JsonObject;
                context = InvokeContext.Deserialize(json);
            }

            UpdateInvokeItems(context);

            bool hasReadyItems = false;
            do
            {
                hasReadyItems = false;

                for (int index = 0; index < context.InvokeItems.Count; index++)
                {
                    var item = context.InvokeItems[index];
                    if (item.Ready && (!item.Completed))
                    {
                        hasReadyItems = true;

                        try
                        {
                            await RunInvokeItem(context, index);
                        }
                        catch(Exception ex)
                        {
                            context.LogTraceError("Failed to run invoke item " + index + " Exception:" + ex.ToString());
                        }

                        item.SetCompleted();
                    }
                }
            } while (hasReadyItems);

            var writer = new JsonWriter();
            InvokeContext.Serialize(writer, context, context.TraceEnabled ? Formats.BatchInvokeResultWithRequest : Formats.BatchInvokeResult);
            return this.JsonResponseJson(0, "InvokeContext", writer.ToString());
        }


        private void UpdateInvokeItems(InvokeContext context)
        {
            var httpContext = HttpContext;
            var userAgent = httpContext.Request.UserAgent ?? string.Empty;
            var clientIPAddress = httpContext.Request.UserHostAddress ?? string.Empty;

            foreach(var item in context.InvokeItems)
            {
                // To avoid infinite loop, ApiProxy cannot invoke another ApiProxy
                if (item.Request.Url.IndexOf("apiproxy/batchinvoke", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    throw new Exception("To avoid infinite loop, ApiProxy cannot invoke another ApiProxy");
                }

                if (string.IsNullOrEmpty(item.Request.Method))
                {
                    item.Request.Method = "GET";
                }

                if (item.Request.Url.StartsWith("~/"))
                {
                    item.Request.Url = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content(item.Request.Url);
                }

                if (!item.Request.Headers.ContainsKey("User-Agent"))
                {
                    item.Request.Headers["User-Agent"] = userAgent;
                }

                item.Request.Headers["X-ApiProxy-Client-IP"] = clientIPAddress;
            }
        }


        private async Task RunInvokeItem(InvokeContext context, int index)
        {
            var item = context.InvokeItems[index];
            context.LogTraceInfo("Start invoke: " + item.Request.Method + " " + item.Request.Url);

            Engine engine = null;
            if (!string.IsNullOrEmpty(item.Script))
            {
                engine = new Engine();
                engine.SetValue("_context", context);
                engine.SetValue("_index", index);
                engine.SetValue("logger", new JSLogger(context));
                engine.Execute("function preprocess(context, index, item) { }\r\n function postprocess(context, index, item) { }");
                engine.Execute(item.Script);
                engine.Execute("preprocess(_context, _index, _context.InvokeItems[_index]);");
            }

            await SendInvoke(context, item);

            if (!string.IsNullOrEmpty(item.Script))
            {
                engine.Execute("postprocess(_context, _index, _context.InvokeItems[_index]);");
            }

            context.LogTraceInfo("End invoke: " + item.Response.StatusCode + " " + item.Request.Method + " " + item.Request.Url);
        }

        private async Task SendInvoke(InvokeContext context, InvokeItem item)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.Request.Url);
            request.Method = item.Request.Method;
            request.Timeout = item.TimeoutInSeconds * 1000;

            foreach (var pair in item.Request.Headers)
            {
                switch (pair.Key.ToLowerInvariant())
                {
                    case "content-type":
                        request.ContentType = pair.Value;
                        break;
                    case "user-agent":
                        request.UserAgent = pair.Value;
                        break;
                    default:
                        request.Headers[pair.Key] = pair.Value;
                        break;
                }
            }

            if (string.CompareOrdinal(request.Method, "GET") != 0)
            {
                var contentData = new List<byte>();
                byte[] data = Encoding.UTF8.GetBytes(item.Request.Body);
                contentData.AddRange(data);

                request.ContentLength = contentData.Count;
                Stream stream = await request.GetRequestStreamAsync();
                stream.Write(contentData.ToArray(), 0, contentData.Count);
                stream.Flush();
            }

            // Start invoking
            try
            {
                var response = (HttpWebResponse)await request.GetResponseAsync();

                item.Response.StatusCode = (int)response.StatusCode;

                item.Response.Headers.Clear();
                foreach (string key in response.Headers.AllKeys)
                {
                    item.Response.Headers[key] = response.Headers[key];
                }

                Stream responseContentStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(responseContentStream))
                {
                    item.Response.Body = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null && ex.Response is HttpWebResponse)
                {
                    var httpWebResponse = ex.Response as HttpWebResponse;
                    item.Response.StatusCode = (int)httpWebResponse.StatusCode;
                }
                var stream = ex.Response.GetResponseStream();
                var message = "Error: status:" + ex.Status + "\r\n" + (stream == null ? string.Empty : new StreamReader(stream).ReadToEnd());
                context.LogTraceError(message + " Exception: " + ex.ToString());
            }
            catch (Exception ex)
            {
                context.LogTraceError("Exception: " + ex.ToString());
            }
        }

    }
}
