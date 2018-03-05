using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WindowsServer.Web.Filters
{
    public class CorsFilter : ActionFilterAttribute
    {
        public bool AllowAnyOrigin { get; set; }
        public IList<string> AllowOrigins { get; set; }
        public string AllowMethods { get; set; }
        public string AllowHeaders { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var origin = httpContext.Request.Headers.Get(CorsConstants.Origin);
            if (origin != null)
            {
                if (AllowAnyOrigin)
                {
                    httpContext.Response.Headers[CorsConstants.AccessControlAllowOrigin] = CorsConstants.AnyOrigin;
                    httpContext.Response.Headers[CorsConstants.AccessControlAllowMethods] = AllowMethods;
                    httpContext.Response.Headers[CorsConstants.AccessControlAllowHeaders] = AllowHeaders;
                }
                else if (AllowOrigins.Contains(origin, StringComparer.InvariantCultureIgnoreCase))
                {
                    httpContext.Response.Headers[CorsConstants.AccessControlAllowOrigin] = origin;
                    httpContext.Response.Headers[CorsConstants.AccessControlAllowMethods] = AllowMethods;
                    httpContext.Response.Headers[CorsConstants.AccessControlAllowHeaders] = AllowHeaders;
                }
                else
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, $"The origin '{origin}' is not allowed.");
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
