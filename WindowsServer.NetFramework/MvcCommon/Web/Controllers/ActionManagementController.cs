using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WindowsServer.Web.Filters;

namespace WindowsServer.Web.Controllers
{
    public class ActionManagementController : Controller
    {
        public ActionResult BrowseLiveActions()
        {
            var liveActions = ActionDiagnoseFilter.GenerateSnapshot();

            var sb = new StringBuilder(4096);
            sb.Append("<html><body>");
            sb.Append("Live Actions:");

            sb.Append("<table><thead><tr><td>ExecutingTime(UTC)</td><td>IpAddress</td><td>Url</td><td>UserAgent</td></tr></thead><tbody>");
            var actions = from a in liveActions orderby a.ActionExecutingTime select a;
            foreach (var action in actions)
            {
                sb.Append("<tr><td>");
                sb.Append(Server.HtmlEncode(action.ActionExecutingTime.ToString("yyyy-MM-dd hh:mm:ss.fff")));
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(action.IpAddress));
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(action.Url));
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(action.UserAgent));
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table></body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }
    }
}
