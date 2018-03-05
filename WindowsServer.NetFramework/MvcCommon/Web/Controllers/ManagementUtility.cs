using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WindowsServer.Web.Controllers
{
    internal static class ManagementUtility
    {
        internal static string BuildHeaderHtml()
        {
            var server = HttpContext.Current.Server;

            // Machine name
            string machineName = string.Empty;
            try
            {
                machineName = server.MachineName;
            }
            catch
            {
                // Sometimes, machine name cannot be found.
                // Do nothing
            }

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("<h8>");
            sb.Append(HttpUtility.HtmlEncode(machineName));
            sb.Append("</h8><br/>");
            return sb.ToString();
        }
    }
}
