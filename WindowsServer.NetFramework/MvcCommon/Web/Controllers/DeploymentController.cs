using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WindowsServer.Web.Controllers
{
    public class DeploymentController : Controller
    {
        public ActionResult GetInformation()
        {
            var root = Server.MapPath("~");

            var maxLastModifiedDate = DateTime.MinValue;

            var binPath = Path.Combine(root, "bin");
            foreach (var filePath in Directory.GetFiles(binPath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.LastWriteTimeUtc > maxLastModifiedDate)
                {
                    maxLastModifiedDate = fileInfo.LastWriteTimeUtc;
                }
            }

            var sb = new StringBuilder(4096);
            RenderBootstrapHtmlPrefix(sb, "Deployement Information");

            sb.Append("<p><h3>Version Code:" + maxLastModifiedDate.ToString("yyyyMMddHHmmss") + "</h3></p>");

            sb.Append("<table class=\"table table-hover\"><thead><tr><td>Assembly</td><td>FullName</td><td>Version</td><td>Size</td><td>Modified Date(UTC)</td><td>Path</td></tr></thead><tbody>");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.GetName().Name))
            {
                sb.Append("<tr><td>");
                sb.Append(assembly.GetName().Name);
                sb.Append("</td><td>");
                sb.Append(assembly.FullName);
                sb.Append("</td><td>");
                sb.Append(assembly.GetName().Version.ToString());
                try
                {
                    sb.Append("</td><td>");
                    var codeBase = assembly.CodeBase;
                    var uri = new UriBuilder(codeBase);
                    var path = Uri.UnescapeDataString(uri.Path);
                    var fileInfo = new FileInfo(path);
                    sb.Append(fileInfo.Length);
                    sb.Append("</td><td>");
                    sb.Append(fileInfo.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss"));
                    sb.Append("</td><td>");
                    sb.Append(path);
                }
                catch(Exception ex)
                {
                    sb.Append("</td><td colspan=\"3\">");
                    sb.Append(ex.Message);
                }
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table>");

            RenderBootstrapHtmlPostfix(sb);

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

        private void RenderBootstrapHtmlPrefix(StringBuilder html, string title)
        {
            html.Append("<!DOCTYPE html><html lang=\"en\"><head><title>");
            html.Append(Server.HtmlEncode(title));
            html.Append("</title><link rel=\"stylesheet\" href=\"http://netdna.bootstrapcdn.com/bootstrap/3.0.3/css/bootstrap.min.css\"><link rel=\"stylesheet\" href=\"http://netdna.bootstrapcdn.com/bootstrap/3.0.3/css/bootstrap-theme.min.css\"></head><body>");
        }

        private void RenderBootstrapHtmlPostfix(StringBuilder html)
        {
            html.Append("<script src=\"https://code.jquery.com/jquery.js\"></script><script src=\"http://netdna.bootstrapcdn.com/bootstrap/3.0.3/js/bootstrap.min.js\"></script></body></html>");
        }
    }
}
