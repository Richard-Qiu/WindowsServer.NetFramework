using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WindowsServer.Web.Filters
{
    public class BaiduYunguanceFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var request = filterContext.HttpContext.Request;
            if (request.HttpMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (request.UserAgent.StartsWith("Baidu-YunGuanCe", StringComparison.InvariantCultureIgnoreCase))
            {
                filterContext.Result = new ContentResult()
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "text/html",
                    Content = "<html><body>The request is filtered by BaiduYunguanceFilter.</body></html>"
                };
            }

        }
    }
}
