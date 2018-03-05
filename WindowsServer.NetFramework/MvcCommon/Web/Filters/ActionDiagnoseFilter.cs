using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WindowsServer.Web.Filters
{
    public class ActionDiagnoseFilter : ActionFilterAttribute
    {
        public class ActionLifeCycle
        {
            public string Key { get; set; }
            public string Url { get; set; }
            public string UserAgent { get; set; }
            public string IpAddress { get; set; }
            public DateTime ActionExecutingTime { get; set; }
            public DateTime ActionExecutedTime { get; set; }
            public DateTime ResultExecutingTime { get; set; }
        }

        private static Dictionary<string, ActionLifeCycle> _lives = new Dictionary<string, ActionLifeCycle>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            string url = filterContext.HttpContext.Request.RawUrl ?? string.Empty;
            string userAgent = filterContext.HttpContext.Request.UserAgent ?? string.Empty;
            string ipAddress = filterContext.HttpContext.Request.UserHostAddress ?? string.Empty;
            string key = ipAddress + url + userAgent;
            lock (_lives)
            {
                _lives.Add(key, new ActionLifeCycle()
                {
                    Key = key,
                    Url = url,
                    UserAgent = userAgent,
                    IpAddress = ipAddress,
                    ActionExecutingTime = DateTime.UtcNow,
                });
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            string url = filterContext.HttpContext.Request.RawUrl ?? string.Empty;
            string userAgent = filterContext.HttpContext.Request.UserAgent ?? string.Empty;
            string ipAddress = filterContext.HttpContext.Request.UserHostAddress ?? string.Empty;
            string key = ipAddress + url + userAgent;
            lock (_lives)
            {
                _lives.Remove(key);
            }
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

        public static List<ActionLifeCycle> GenerateSnapshot()
        {
            var lives = new List<ActionLifeCycle>();
            lock (_lives)
            {
                foreach (var life in _lives.Values)
                {
                    lives.Add(new ActionLifeCycle()
                    {
                        Key = life.Key,
                        Url = life.Url,
                        UserAgent = life.UserAgent,
                        IpAddress = life.IpAddress,
                        ActionExecutingTime = life.ActionExecutingTime,
                        ActionExecutedTime = life.ActionExecutedTime,
                        ResultExecutingTime = life.ResultExecutingTime
                    });
                }
            }
            return lives;
        }
    }
}
