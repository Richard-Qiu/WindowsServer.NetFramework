using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WindowsServer.Web.Filters
{

    public class WebApiRequestTrackerFilter : ActionFilterAttribute
    {
        internal static Dictionary<HttpActionContext, RequestTrackerFilter.RequestData> _requestMap = new Dictionary<HttpActionContext, RequestTrackerFilter.RequestData>();

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            HttpContextWrapper httpContext = actionContext.Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            var request = httpContext.Request;

            var data = new RequestTrackerFilter.RequestData()
            {
                Id = Guid.NewGuid(),
                Context = actionContext,
                Method = request.HttpMethod ?? string.Empty,
                Url = request.RawUrl ?? string.Empty,
                UserAgent = request.UserAgent ?? string.Empty,
                IpAddress = request.UserHostAddress ?? string.Empty,
                RequestHeaders = new Dictionary<string, string>(),
                RequestBodyLength = request.ContentLength,
                RequestBody = null,
                ResponseHeaders = new Dictionary<string, string>(),
                ResponseBody = null,
                ActionExecutingTime = DateTime.UtcNow,
            };

            foreach (var key in request.Headers.AllKeys)
            {
                data.RequestHeaders[key] = request.Headers[key];
            }

            var stream = request.InputStream;
            if (stream.CanSeek)
            {
                long previousPosition = stream.Position;
                using (var memoryStream = new MemoryStream())
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

            lock (RequestTrackerFilter._requests)
            {
                if (RequestTrackerFilter._requests.Count >= RequestTrackerFilter._tracedRequestLimit)
                {
                    RequestTrackerFilter.Recycle();
                }
                RequestTrackerFilter._requests.Add(data.Id, data);
                _requestMap.Add(actionContext, data);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            RequestTrackerFilter.RequestData data;
            lock (RequestTrackerFilter._requests)
            {
                if (!_requestMap.TryGetValue(actionExecutedContext.ActionContext, out data))
                {
                    return;
                }
                _requestMap.Remove(actionExecutedContext.ActionContext);
            }

            DateTime utcNow = DateTime.UtcNow;
            data.ActionExecutedTime = utcNow;
            data.ResultExecutingTime = utcNow;
            data.ResultExecutedTime = utcNow;

            if (actionExecutedContext.Exception != null)
            {
                data.Exception = actionExecutedContext.Exception.ToString();

                if (actionExecutedContext.Response != null)
                {
                    data.ResponseHttpCode = (int)actionExecutedContext.Response.StatusCode;
                }

                if (data.ResponseHttpCode == 0)
                {
                    var httpException = actionExecutedContext.Exception as HttpException;
                    if (httpException != null)
                    {
                        data.ResponseHttpCode = httpException.GetHttpCode();
                    }
                }
            }
            else
            {
                data.Exception = string.Empty;

                data.ResponseHttpCode = (int)actionExecutedContext.Response.StatusCode;
                foreach (var pair in actionExecutedContext.Response.Headers)
                {
                    data.ResponseHeaders[pair.Key] = string.Join("; ", pair.Value.ToList());
                }

                if (actionExecutedContext.Response.Content == null)
                {
                    data.ResponseBody = new byte[0];
                    data.ResponseBodyLength = data.ResponseBody.Length;
                }
                else
                {
                    var readTask = actionExecutedContext.Response.Content.ReadAsStreamAsync();
                    readTask.Wait();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        readTask.Result.CopyTo(ms);
                        ms.Flush();
                        data.ResponseBody = ms.ToArray();
                        data.ResponseBodyLength = data.ResponseBody.Length;
                    }
                }
            }

        }


    }
}
