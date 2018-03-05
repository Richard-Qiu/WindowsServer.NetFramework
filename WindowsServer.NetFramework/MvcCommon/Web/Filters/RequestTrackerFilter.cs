using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WindowsServer.Configuration;
using WindowsServer.Json;

namespace WindowsServer.Web.Filters
{
    public class RequestTrackerFilter : ActionFilterAttribute
    {

        internal static readonly int _tracedRequestLimit = int.Parse(ConfigurationCenter.Global["RequestTracker.TracedRequestLimit"] ?? "10000");

        public class RequestData
        {
            public Guid Id { get; set; }
            public object Context { get; set; }
            public string Method { get; set; }
            public string Url { get; set; }
            public string UserAgent { get; set; }
            public string IpAddress { get; set; }
            public Dictionary<string, string> RequestHeaders { get; set; }
            public long RequestBodyLength { get; set; }
            public byte[] RequestBody { get; set; }
            public int ResponseHttpCode { get; set; }
            public string Exception { get; set; }
            public Dictionary<string, string> ResponseHeaders { get; set; }
            public long ResponseBodyLength { get; set; }
            public byte[] ResponseBody { get; set; }
            public DateTime ActionExecutingTime { get; set; }
            public DateTime ActionExecutedTime { get; set; }
            public DateTime ResultExecutingTime { get; set; }
            public DateTime ResultExecutedTime { get; set; }

            private string _accessToken = null;
            public string AccessToken
            {
                get
                {
                    if (_accessToken == null)
                    {
                        if (!RequestHeaders.TryGetValue("AccessToken", out _accessToken))
                        {
                            _accessToken = string.Empty;
                        }
                    }
                    return _accessToken;
                }
            }
        }

        internal static Dictionary<Guid, RequestData> _requests = new Dictionary<Guid, RequestData>();
        private static Dictionary<HttpContextBase, RequestData> _requestMap = new Dictionary<HttpContextBase, RequestData>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var data = new RequestData()
            {
                Id = Guid.NewGuid(),
                Context = filterContext,
                Method = filterContext.HttpContext.Request.HttpMethod ?? string.Empty,
                Url = filterContext.HttpContext.Request.RawUrl ?? string.Empty,
                UserAgent = filterContext.HttpContext.Request.UserAgent ?? string.Empty,
                IpAddress = filterContext.HttpContext.Request.UserHostAddress ?? string.Empty,
                RequestHeaders = new Dictionary<string,string>(),
                RequestBodyLength = filterContext.HttpContext.Request.ContentLength,
                RequestBody = null,
                ResponseHeaders = new Dictionary<string,string>(),
                ResponseBody = null,
                ActionExecutingTime = DateTime.UtcNow,
            };

            foreach (var key in filterContext.HttpContext.Request.Headers.AllKeys)
            {
                data.RequestHeaders[key] = filterContext.HttpContext.Request.Headers[key];
            }

            var stream = filterContext.HttpContext.Request.InputStream;
            if (stream.CanSeek)
            {
                long previousPosition = stream.Position;
                using(var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Flush();

                    data.RequestBody = new byte[memoryStream.Length];
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.Read(data.RequestBody, 0, data.RequestBody.Length);
                }
                stream.Seek(previousPosition, SeekOrigin.Begin);
            }
            else
            {
                data.RequestBody = Encoding.UTF8.GetBytes("The request input stream cannot seek.");
            }

            lock(_requests)
            {
                if (_requests.Count >= _tracedRequestLimit)
                {
                    Recycle();
                }
                _requests.Add(data.Id, data);
                _requestMap.Add(filterContext.HttpContext, data);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            RequestData data;
            lock(_requests)
            {
                if (!_requestMap.TryGetValue(filterContext.HttpContext, out data))
                {
                    return;
                }
            }

            data.ActionExecutedTime = DateTime.UtcNow;
            data.Exception = filterContext.Exception != null ? filterContext.Exception.ToString() : string.Empty;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            RequestData data;
            lock(_requests)
            {
                if (!_requestMap.TryGetValue(filterContext.HttpContext, out data))
                {
                    return;
                }
            }

            data.ResultExecutingTime = DateTime.UtcNow;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);

            RequestData data;
            lock(_requests)
            {
                if (!_requestMap.TryGetValue(filterContext.HttpContext, out data))
                {
                    return;
                }
                _requestMap.Remove(filterContext.HttpContext);
            }

            data.ResultExecutedTime = DateTime.UtcNow;

            data.ResponseHttpCode = filterContext.HttpContext.Response.StatusCode;
            data.Exception = filterContext.Exception != null ? filterContext.Exception.ToString() : string.Empty;

            foreach (var key in filterContext.HttpContext.Response.Headers.AllKeys)
            {
                data.ResponseHeaders[key] = filterContext.HttpContext.Response.Headers[key];
            }

            var result = filterContext.Result;
            if (result is FileContentResult)
            {
                var fileContentResult = (FileContentResult)result;
                data.ResponseBodyLength = fileContentResult.FileContents.Length;
                if ((!string.IsNullOrEmpty(fileContentResult.ContentType) && fileContentResult.ContentType.Contains("stream")))
                {
                    data.ResponseBody = new byte[0];
                }
                else
                {
                    data.ResponseBody = fileContentResult.FileContents;
                }
            }
            else if (result is ContentResult)
            {
                var contentResult = (ContentResult)result;
                if (contentResult.ContentEncoding != null)
                {
                    data.ResponseBody = contentResult.ContentEncoding.GetBytes(contentResult.Content);
                }
                else
                {
                    data.ResponseBody = Encoding.UTF8.GetBytes(contentResult.Content);
                }
                data.ResponseBodyLength = data.ResponseBody.Length;
            }
            else if (result is JsonResult)
            {
                var jsonResult = (JsonResult)result;
                if (jsonResult.Data == null)
                {
                    data.ResponseBody = new byte[0];
                    data.ResponseBodyLength = data.ResponseBody.Length;
                }
                else
                {
                    try
                    {
                        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(jsonResult.Data.GetType());
                        using (MemoryStream ms = new MemoryStream())
                        {
                            jsonSerializer.WriteObject(ms, jsonResult.Data);
                            ms.Flush();
                            data.ResponseBody = ms.ToArray();
                            data.ResponseBodyLength = data.ResponseBody.Length;
                        }
                    }
                    catch(Exception ex)
                    {
                        data.ResponseBody = Encoding.UTF8.GetBytes("Failed to serialize data object." + jsonResult.Data.GetType() + ex.ToString());
                        data.ResponseBodyLength = -1;
                    }
                }
            }
            else
            {
                data.ResponseBody = Encoding.UTF8.GetBytes("The response result is unknown. " + result.GetType().ToString());
                data.ResponseBodyLength = -1;
            }
        }

        public static List<RequestData> GenerateSnapshot(int? count, string ipAddress, string userAgent, string accessToken)
        {
            var lives = new List<RequestData>();
            lock (_requests)
            {
                var query = _requests.Values.AsQueryable();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    query = query.Where(r => r.IpAddress == ipAddress);
                }
                if (!string.IsNullOrEmpty(userAgent))
                {
                    query = query.Where(r => r.UserAgent == userAgent);
                }
                if (!string.IsNullOrEmpty(accessToken))
                {
                    query = query.Where(r => r.AccessToken.Contains(accessToken));
                }

                query = query.OrderByDescending(r => r.ActionExecutingTime);

                if (count.HasValue)
                {
                    query = query.Take(count.Value);
                }

                return query.ToList();
            }
        }

        public static RequestData GetRequestData(Guid requestId)
        {
            lock (_requests)
            {
                RequestData requestData;
                _requests.TryGetValue(requestId, out requestData);
                return requestData;
            }
        }

        internal static void Recycle()
        {
            lock (_requests)
            {
                var count = _requests.Count / 2;
                var recyclingRequests = (from r in _requests orderby r.Value.ActionExecutingTime select r).Take(count);
                foreach (var request in recyclingRequests)
                {
                    _requests.Remove(request.Key);
                } 
            }
        }

    }
}
