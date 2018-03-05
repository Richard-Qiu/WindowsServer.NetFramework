using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web.Caching;
using System.Web.Mvc;
using WindowsServer.Caching;
using WindowsServer.Log;
using WindowsServer.Web.Caching;

namespace WindowsServer.Web.Controllers
{
    public class CacheManagementController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ActionResult DisplayCacheHits(string order)
        {
            var hitsSnapshot = CacheManager.GenerateHitsSnapshot();

            var sb = new StringBuilder(4096);
            sb.Append("<html><body>");
            sb.Append("Diagnostic:");
            if (!CacheManager.IsDiagnosing)
            {
                sb.Append("<a href=\"");
                sb.Append(Url.Action("StartDiagnosing"));
                sb.Append("\">Start</a>&nbsp;&nbsp;&nbsp;");
            }
            else
            {
                sb.Append("<a href=\"");
                sb.Append(Url.Action("StopDiagnosing"));
                sb.Append("\">Stop</a>");
            }
            sb.Append("<br />");
            sb.Append("<a href=\"");
            sb.Append(Url.Action("DisplayCacheHits", new { order = "KeyAscending" }));
            sb.Append("\">Key&nbsp;Ascending</a>&nbsp;&nbsp;&nbsp;");
            sb.Append("<a href=\"");
            sb.Append(Url.Action("DisplayCacheHits", new { order = "HitCountDescending" }));
            sb.Append("\">HitCount&nbsp;Descending</a><br />");

            if (CacheManager.DiagnoseStartDate != DateTime.MinValue)
            {
                sb.Append("Last start date(UTC):&nbsp;");
                sb.Append(Server.HtmlEncode(CacheManager.DiagnoseStartDate.ToString()));
                sb.Append("<br />");
            }
            if (CacheManager.DiagnoseStopDate != DateTime.MinValue)
            {
                sb.Append("Last stop date(UTC):&nbsp;");
                sb.Append(Server.HtmlEncode(CacheManager.DiagnoseStopDate.ToString()));
                sb.Append("<br />");
            }

            sb.Append("<table><thead><tr><td>Name</td><td>Count</td></tr></thead><tbody>");
            var hits =
                (string.Compare(order, "HitCountDescending", StringComparison.InvariantCultureIgnoreCase) != 0)
                ? (from p in hitsSnapshot.Hits orderby p.Key ascending select p)
                : (from p in hitsSnapshot.Hits orderby p.Value descending select p);
            foreach(var hitItem in hits)
            {
                sb.Append("<tr><td>");
                sb.Append(Server.HtmlEncode(hitItem.Key));
                sb.Append("</td><td>");
                sb.Append(hitItem.Value);
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table></body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

        public ActionResult StartDiagnosing()
        {
            CacheManager.StartDiagnosing();

            var sb = new StringBuilder(4096);
            sb.Append("<html><body>Started successfully.<br />");
            sb.Append("<a href=\"");
            sb.Append(Url.Action("DisplayCacheHits"));
            sb.Append("\">Back</a>");
            sb.Append("</body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

        public ActionResult StopDiagnosing()
        {
            CacheManager.StopDiagnosing();

            var sb = new StringBuilder(4096);
            sb.Append("<html><body>Stopped successfully.<br />");
            sb.Append("<a href=\"");
            sb.Append(Url.Action("DisplayCacheHits"));
            sb.Append("\">Back</a>");
            sb.Append("</body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }


        public ActionResult Peek(int? count, string keywords)
        {
            if (!count.HasValue || count.Value <= 0)
            {
                count = 100;
            }

            var cacheItems = FilterCacheItems(count.Value, keywords);

            var sb = new StringBuilder(4096);
            RenderBootstrapHtmlPrefix(sb, "Cache Items");

            sb.Append("<p><h3>Cache Items Count:" + cacheItems.Count + "<a href=\"../WebCache/ClearAllCache\" target=\"_blank\" style=\"margin-left: 24px;font-size: 18px;\">Clear All Cache</a></h3></p>");
            sb.Append("<p><form class=\"form-inline\" role=\"form\" method=\"post\">");

            sb.Append("<div class=\"form-group\" style=\"margin-left:20px;\"><label for=\"countInput\" class=\"control-label\">Count:</label></div><div class=\"form-group\"><input type=\"number\" class=\"form-control\" id=\"countInput\" placeholder=\"Count\" name=\"count\" value=\"");
            sb.Append(WebUtility.HtmlDecode(count.HasValue ? count.Value.ToString() : string.Empty));
            sb.Append("\"></div>");

            sb.Append("<div class=\"form-group\" style=\"margin-left:20px;\"><label for=\"keywords\" class=\"control-label\">Keywords:</label></div><div class=\"form-group\"><input type=\"text\" class=\"form-control\" id=\"keywords\" placeholder=\"Keywords\" name=\"keywords\" value=\"");
            sb.Append(keywords ?? string.Empty);
            sb.Append("\"></div>");

            sb.Append("<button type=\"submit\" class=\"btn btn-primary\" style=\"margin-left:15px;\">Query</button></form></p>");

            var index = 1;
            sb.Append("<table class=\"table\">");
            sb.Append("<tr><th>#</th><th>Key</th><th>Value</th></tr>");
            foreach (var item in cacheItems)
            {
                sb.Append("<tr>");
                sb.AppendFormat("<td>{0}</td>", index);
                sb.AppendFormat("<td>{0}</td>", WebUtility.HtmlDecode(item.Key.ToString()));
                sb.AppendFormat("<td>{0}</td>", WebUtility.HtmlDecode(item.Value.ToString()));
                sb.Append("</tr>");
                index++;
            }
            sb.Append("</table>");
            RenderBootstrapHtmlPostfix(sb);

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

        private List<KeyValuePair<string, object>> FilterCacheItems(int count, string keywords)
        {
            var result = new List<KeyValuePair<string, object>>();

            var index = 1;
            var cacheItems = CacheManager.GetEnumerator();
            while (cacheItems.MoveNext())
            {
                if (index > count)
                {
                    break;
                }

                var pair = FormatCacheItem(cacheItems.Current);
                if (string.IsNullOrEmpty(keywords))
                {
                    result.Add(pair);
                }
                else
                {
                    if (pair.Key.IndexOf(keywords, StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        continue;
                    }
                    result.Add(pair);
                }
                index++;
            }
            return result;
        }

        private KeyValuePair<string, object> FormatCacheItem(object cacheItem)
        {
            string key;
            object value;
            
            if (cacheItem is DictionaryEntry)   // Asp.net Cache
            {
                var entry = (DictionaryEntry)cacheItem;
                key = entry.Key.ToString();
                value = entry.Value;
            }
            else if (cacheItem is KeyValuePair<string, object>)
            {
                var pair = (KeyValuePair<string, object>)cacheItem;
                key = pair.Key;
                value = pair.Value;
            }
            else
            {
                key = "Unrecognized cache item type: " + cacheItem.GetType();
                value = cacheItem;
            }

            if (!((value is string)
               || (value is Guid)
               || (value is DateTime)
               || (value is Int32)
               || (value is Int64)
               || (value is Boolean)
               || (value is Double)
               || (value is Single)))
            {
                var v = new StringBuilder();
                var type = value.GetType();
                v.AppendFormat("<strong>Class:</strong> {0}", type);
                v.Append("<br />");

                v.Append("<strong>Properties:</strong>");
                try
                {
                    foreach (var p in type.GetProperties())
                    {
                        var propertyName = WebUtility.HtmlDecode(p.Name);
                        var propertyValue = WebUtility.HtmlDecode(p.GetValue(value).ToString());
                        v.AppendFormat("<span style=\"display:block;text-indent:24px;\">{0}: {1}</span>", propertyName, propertyValue);
                    }
                }
                catch (Exception ex)
                {
                    v.AppendFormat("<span style=\"display:block;text-indent:24px;color:red;\">Exception: {0}</span>", WebUtility.HtmlDecode(ex.ToString()));
                }
                value = v.ToString();
            }
            return new KeyValuePair<string, object>(key, value);
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
