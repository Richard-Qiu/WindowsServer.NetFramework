using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using WindowsServer.Log;

namespace WindowsServer.Web
{
    public class WebApiGlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                var httpContextString = GetHttpContextString(context);

                var exception = context.Exception;
                var httpException = exception as HttpException;

                if (httpException != null)
                {
                    int statusCode = httpException.GetHttpCode();
                    if (statusCode < 500)
                    {
                        _logger.WarnException("HttpException is detected.[" + httpContextString + "]", httpException);
                    }
                    else
                    {
                        _logger.ErrorException("HttpException is detected.[" + httpContextString + "]", httpException);
                    }

                    context.Response = new HttpResponseMessage((HttpStatusCode)httpException.GetHttpCode());
                    return;
                }

                _logger.ErrorException("Uncaught exception is detected.[" + httpContextString + "]", exception);
            }
        }

        private static string GetHttpContextString(HttpActionExecutedContext context)
        {
            if (context.Request != null)
            {
                HttpContextWrapper httpContext = context.Request.Properties["MS_HttpContext"] as HttpContextWrapper;

                var request = context.Request;
                var rawUrl = request.RequestUri.ToString() ?? string.Empty;

                var clientIp = string.Empty;
                var userAgent = string.Empty;
                if (httpContext != null)
                {
                    clientIp = httpContext.Request.UserHostAddress ?? string.Empty;
                    userAgent = httpContext.Request.UserAgent ?? string.Empty;
                }

                return string.Format(
                    "ClientIP:{0} RawUrl:{1} UserAgent:{2}",
                    clientIp,
                    rawUrl,
                    userAgent);
            }
            return string.Empty;
        }
    }
}
