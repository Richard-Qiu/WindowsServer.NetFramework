using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WindowsServer.Configuration;

namespace WindowsServer.Web.Controllers
{
    public class SettingsManagementController : Controller
    {
        public ActionResult BrowseSettings()
        {
            ManagementConsoleAuthentication.VerifyTokenCookie(this);

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("<html><body>");
            sb.Append(ManagementUtility.BuildHeaderHtml());
            // Local
            sb.Append("<h3>Local</h3><br/>");
            sb.Append("<table>");
            foreach (var key in ConfigurationCenter.Local.DiscoverAllKeys())
            {
                sb.Append("<tr><td>");
                sb.Append(HttpUtility.HtmlEncode(key));
                sb.Append("</td><td>");
                sb.Append(ConfigurationCenter.Local[key] ?? "(null)");
                sb.Append("</td></tr>");
            }
            sb.Append("</table><br/>");

            // Global
            sb.Append("<h3>Global</h3><br/>");
            sb.Append("<table>");
            foreach (var key in ConfigurationCenter.Global.DiscoverAllKeys())
            {
                sb.Append("<tr><td>");
                sb.Append(HttpUtility.HtmlEncode(key));
                sb.Append("</td><td>");
                sb.Append(ConfigurationCenter.Global[key] ?? "(null)");
                sb.Append("</td></tr>");
            }
            sb.Append("</table><br/>");

            sb.Append("</body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }
    }
}
