using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using WindowsServer.Log;

namespace WindowsServer.Web
{
    public class GlobalExceptionHandler : IExceptionFilter
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void OnException(ExceptionContext filterContext)
        {
            Exception ex = filterContext.Exception;
            HttpException httpException = ex as HttpException;

            var httpContextString = GetHttpContextString(filterContext);
            if (httpException != null)
            {
                int statusCode = httpException.GetHttpCode();
                bool isFromSL = filterContext.HttpContext.Request.Headers.AllKeys.Any(item => item == "Silverlight");
                if (statusCode == 401 && isFromSL)
                {

                    filterContext.HttpContext.Response.StatusCode = 200;

                    filterContext.HttpContext.Response.ContentType = "slerror";
                    filterContext.HttpContext.Response.Output.WriteLine(httpException.Message);
                    filterContext.ExceptionHandled = true;
                    _logger.WarnException("HttpException is detected.[" + httpContextString + "]", ex);

                    return;
                }
                else if (statusCode < 500)
                {
                    filterContext.HttpContext.Response.StatusCode = statusCode;

                    filterContext.HttpContext.Response.Output.WriteLine(httpException.Message);
                    filterContext.ExceptionHandled = true;
                    _logger.WarnException("HttpException is detected.[" + httpContextString + "]", ex);
                    return;
                }
            }
          
            _logger.ErrorException("Uncaught exception is detected.[" + httpContextString + "]", ex);
        }

        private static string GetHttpContextString(ExceptionContext filterContext)
        {
            if ((filterContext.HttpContext != null) && (filterContext.HttpContext.Request != null))
            {
                var request = filterContext.HttpContext.Request;
                return string.Format(
                    "ClientIP:{0} RawUrl:{1} UserAgent:{2}",
                    request.UserHostAddress ?? string.Empty,
                    request.RawUrl ?? string.Empty,
                    request.UserAgent ?? string.Empty);
            }
            return string.Empty;
        }

        internal static void Initialize(HttpApplication application)
        {
            application.Error += new EventHandler(application_Error);
            GlobalFilters.Filters.Add(new GlobalExceptionHandler());
        }

        static void application_Error(object sender, EventArgs e)
        {
            Exception ex = ((HttpApplication)sender).Server.GetLastError();

            if (ex != null)
            {
                _logger.ErrorException("Uncaught application exception is detected.", ex);
            }
        }
    }
}
