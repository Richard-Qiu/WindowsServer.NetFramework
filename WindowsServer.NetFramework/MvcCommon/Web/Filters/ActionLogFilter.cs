using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WindowsServer.Log;

namespace WindowsServer.Web.Filters
{
    public class ActionLogFilter : ActionFilterAttribute
    {
        public enum ActionLogLevel
        {
            Debug,
            Info,
            Warn,
            Error,
        };

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private ActionLogLevel _logLevel;

        public ActionLogFilter(ActionLogLevel logLevel = ActionLogLevel.Debug)
        {
            _logLevel = logLevel;
        }

        private void Log(string stage, string ipAddress, string url, string userAgent)
        {
            string s = string.Format("{0}   IP:{1} URL:{2} UserAgent:{3}", stage, ipAddress, url, userAgent);
            switch(_logLevel)
            {
                case ActionLogLevel.Debug:
                    _logger.Debug(s);
                    break;
                case ActionLogLevel.Info:
                    _logger.Info(s);
                    break;
                case ActionLogLevel.Warn:
                    _logger.Warn(s);
                    break;
                case ActionLogLevel.Error:
                    _logger.Error(s);
                    break;
                default:
                    // Do not throw exception here, because it is a filter, the failure should not impact the whole application.
                    break;
            }
        }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string url = filterContext.HttpContext.Request.RawUrl ?? string.Empty;
            string userAgent = filterContext.HttpContext.Request.UserAgent ?? string.Empty;
            string ipAddress = filterContext.HttpContext.Request.UserHostAddress ?? string.Empty;

            Log("OnActionExecuting", ipAddress, url, userAgent);

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            string url = filterContext.HttpContext.Request.RawUrl ?? string.Empty;
            string userAgent = filterContext.HttpContext.Request.UserAgent ?? string.Empty;
            string ipAddress = filterContext.HttpContext.Request.UserHostAddress ?? string.Empty;

            Log("OnActionExecuted", ipAddress, url, userAgent);

            base.OnActionExecuted(filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            string url = filterContext.HttpContext.Request.RawUrl ?? string.Empty;
            string userAgent = filterContext.HttpContext.Request.UserAgent ?? string.Empty;
            string ipAddress = filterContext.HttpContext.Request.UserHostAddress ?? string.Empty;

            Log("OnResultExecuting", ipAddress, url, userAgent);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            string url = filterContext.HttpContext.Request.RawUrl ?? string.Empty;
            string userAgent = filterContext.HttpContext.Request.UserAgent ?? string.Empty;
            string ipAddress = filterContext.HttpContext.Request.UserHostAddress ?? string.Empty;

            Log("OnResultExecuted", ipAddress, url, userAgent);
        }

    }
}
