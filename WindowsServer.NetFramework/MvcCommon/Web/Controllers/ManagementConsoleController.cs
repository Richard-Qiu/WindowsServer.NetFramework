using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace WindowsServer.Web.Controllers
{
    public class ManagementConsoleController : Controller
    {
        public ActionResult SignIn()
        {
            var sb = new StringBuilder();
            sb.Append("<html><body>");
            sb.Append(ManagementUtility.BuildHeaderHtml());
            sb.Append("<form method=\"post\" action=\"");
            sb.Append(Url.Action("GetPortal"));
            sb.Append("\" >PIN:<input id=\"pin\" name=\"pin\" type=\"password\" /><input type=\"submit\" value=\"SignIn\" /></form>");
            sb.Append("</body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

        public ActionResult GetPortal(string pin)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body>");
            sb.Append(ManagementUtility.BuildHeaderHtml());
            
            sb.Append("<a href=\"");
            sb.Append(Url.Action("DisplayCacheHits", "CacheManagement"));
            sb.Append("\">Cache</a><br/>");
            
            sb.Append("<a href=\"");
            sb.Append(Url.Action("BrowseDirectory", "FilesManagement"));
            sb.Append("\">File</a><br/>");

            sb.Append("<a href=\"");
            sb.Append(Url.Action("BrowseConnections", "DBManagement"));
            sb.Append("\">DB</a><br/>");

            sb.Append("<a href=\"");
            sb.Append(Url.Action("BrowseSettings", "SettingsManagement"));
            sb.Append("\">Setting</a><br/>");

            sb.Append("<a href=\"");
            sb.Append(Url.Action("BrowseLiveActions", "ActionManagement"));
            sb.Append("\">LiveActions</a><br/>");

            sb.Append("<a href=\"");
            sb.Append(Url.Action("BrowseRequests", "RequestTracker"));
            sb.Append("\">RequestTracker</a><br/>");

            sb.Append("</body></html>");

            var cookie = ManagementConsoleAuthentication.GenerateTokenCookie(pin);
            this.HttpContext.Response.Cookies.Add(cookie);

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }
    }
}
