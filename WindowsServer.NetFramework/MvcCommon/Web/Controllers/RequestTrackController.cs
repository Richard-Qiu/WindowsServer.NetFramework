using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WindowsServer.Web.Filters;

namespace WindowsServer.Web.Controllers
{
    public class RequestTrackerController : Controller
    {
        public ActionResult BrowseRequests(int? count, string ipAddress, string userAgent, string accessToken)
        {
            if (HttpContext.Request.HttpMethod == "GET")
            {
                if (!count.HasValue)
                {
                    count = 100;
                }
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = HttpContext.Request.UserHostAddress ?? string.Empty;
                }
            }

            var requests = RequestTrackerFilter.GenerateSnapshot(count, ipAddress, userAgent, accessToken);

            var sb = new StringBuilder(4096);
            RenderBootstrapHtmlPrefix(sb, "Request Tracker");

            sb.Append("<p><h3>Request Count:" + requests.Count + "</h3></p>");
            sb.Append("<p><form class=\"form-inline\" role=\"form\" method=\"post\">");

            sb.Append("<div class=\"form-group\" style=\"margin-left:20px;\"><label for=\"countInput\" class=\"control-label\">Count:</label></div><div class=\"form-group\"><input type=\"number\" class=\"form-control\" id=\"countInput\" placeholder=\"Count\" name=\"count\" value=\"");
            sb.Append(count.HasValue ? count.Value.ToString() : string.Empty);
            sb.Append("\"></div>");

            sb.Append("<div class=\"form-group\" style=\"margin-left:20px;\"><label for=\"ipInput\" class=\"control-label\">IP Address:</label></div><div class=\"form-group\"><input type=\"text\" class=\"form-control\" id=\"ipInput\" placeholder=\"IP address\" name=\"ipAddress\" value=\"");
            sb.Append(ipAddress ?? string.Empty);
            sb.Append("\"></div>");

            sb.Append("<div class=\"form-group\" style=\"margin-left:20px;\"><label for=\"userAgentInput\" class=\"control-label\">User Agent:</label></div><div class=\"form-group\"><input type=\"text\" class=\"form-control\" id=\"userAgentInput\" placeholder=\"User Agent\" name=\"userAgent\" value=\"");
            sb.Append(userAgent ?? string.Empty);
            sb.Append("\"></div>");

            sb.Append("<div class=\"form-group\" style=\"margin-left:20px;\"><label for=\"accessTokenInput\" class=\"control-label\">Access Token:</label></div><div class=\"form-group\"><input type=\"text\" class=\"form-control\" id=\"accessTokenInput\" placeholder=\"Access Token\" name=\"accessToken\" value=\"");
            sb.Append(accessToken ?? string.Empty);
            sb.Append("\"></div>");

            sb.Append("<button type=\"submit\" class=\"btn btn-primary\" style=\"margin-left:15px;\">Query</button></form></p>");

            sb.Append("<table class=\"table table-hover\"><thead><tr><td></td><td>ExecutingTime(UTC)</td><td>Duration(ms)</td><td>IpAddress</td><td>Method</td><td>RequestLen</td><td>StatusCode</td><td>ResponseLen</td><td>Exception</td><td>Url</td><td>UserAgent</td><td>AccessToken</td></tr></thead><tbody>");
            foreach (var request in requests)
            {
                sb.Append("<tr");
                if ((request.ResponseHttpCode >= 500) || (!string.IsNullOrEmpty(request.Exception)))
                {
                    sb.Append(" class=\"danger\"");
                }
                else if ((request.ResponseHttpCode < 200) || (request.ResponseHttpCode >= 300))
                {
                    sb.Append(" class=\"warning\"");
                }
                sb.Append("><td>");
                sb.Append("<a class=\"btn btn-default btn-xs\" target=\"_blank\" href=\"");
                sb.Append(Server.HtmlEncode(UrlHelper.GenerateContentUrl("~/RequestTracker/BR/" + request.Id.ToString("N"), this.HttpContext)));
                sb.Append("\">Detail</a>");
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(request.ActionExecutingTime.ToString("yyyy-MM-dd hh:mm:ss.fff")));
                sb.Append("</td><td>");
                if (request.ResultExecutedTime == DateTime.MinValue)
                {
                    if (request.ActionExecutedTime == DateTime.MinValue)
                    {
                        sb.Append("Processing Action");
                    }
                    else
                    {
                        sb.Append("Processing Result");
                    }
                }
                else
                {
                    sb.Append((request.ResultExecutedTime - request.ActionExecutingTime).TotalMilliseconds);
                }
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(request.IpAddress));
                sb.Append("</td><td>");
                sb.Append(request.Method);
                sb.Append("</td><td>");
                sb.Append(request.RequestBodyLength);
                sb.Append("</td><td>");
                sb.Append(request.ResponseHttpCode);
                sb.Append("</td><td>");
                sb.Append(request.ResponseBodyLength);
                sb.Append("</td><td>");
                sb.Append(string.IsNullOrEmpty(request.Exception) ? string.Empty : "Yes");
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(request.Url));
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(request.UserAgent));
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(request.AccessToken));
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table>");

            RenderBootstrapHtmlPostfix(sb);

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

        public ActionResult BrowseRequest(Guid id)
        {
            var request = RequestTrackerFilter.GetRequestData(id);
            if (request == null)
            {
                return new HttpNotFoundResult("Cannot find the request.");
            }

            var sb = new StringBuilder(4096);
            RenderBootstrapHtmlPrefix(sb, request.Method + ": " + request.Url);
            sb.Append("<p><h1>");
            sb.Append(Server.HtmlEncode(request.Method));
            sb.Append(": ");
            sb.Append(Server.HtmlEncode(request.Url));
            sb.Append("&nbsp;&nbsp;&nbsp;<small>");
            sb.Append(request.Id.ToString("N"));
            sb.Append("</small>");
            sb.Append("</h1></p>");

            sb.Append("<p><h2>UserAgent:&nbsp;&nbsp;&nbsp;");
            sb.Append(Server.HtmlEncode(request.UserAgent));
            sb.Append("</h2></p>");

            sb.Append("<p><h2>IP Address:&nbsp;&nbsp;&nbsp;");
            sb.Append(Server.HtmlEncode(request.IpAddress));
            sb.Append("</h2></p>");

            sb.Append("<p><h2>Request Headers</h2><br/>");
            sb.Append("<table class=\"table table-hover\"><thead><tr><td>Key</td><td>Value</td></tr></thead><tbody>");
            foreach(var pair in request.RequestHeaders)
            {
                sb.Append("<tr><td>");
                sb.Append(Server.HtmlEncode(pair.Key));
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(pair.Value));
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
            sb.Append("</p>");

            sb.Append("<p><h2>Request Body:&nbsp;");
            sb.Append(request.RequestBodyLength);
            sb.Append(" bytes</h2><br/>");
            sb.Append("<textarea class=\"form-control\" rows=\"10\" style=\"margin-right:20px;\">");
            sb.Append(Server.HtmlEncode(Encoding.UTF8.GetString(request.RequestBody)));
            sb.Append("</textarea></p>");

            sb.Append("<p><h2>Time:&nbsp;");
            sb.Append((request.ResultExecutedTime - request.ActionExecutingTime).TotalMilliseconds);
            sb.Append("ms</h2><br/>ActionExecutingTime(UTC):&nbsp;");
            sb.Append(Server.HtmlEncode(request.ActionExecutingTime.ToString("yyyy-MM-dd hh:mm:ss.fff")));
            sb.Append("<br/>ActionExecutedTime(UTC):&nbsp;");
            sb.Append(Server.HtmlEncode(request.ActionExecutedTime.ToString("yyyy-MM-dd hh:mm:ss.fff")));
            sb.Append("<br/>ResultExecutingTime(UTC):&nbsp;");
            sb.Append(Server.HtmlEncode(request.ResultExecutingTime.ToString("yyyy-MM-dd hh:mm:ss.fff")));
            sb.Append("<br/>ResultExecutedTime(UTC):&nbsp;");
            sb.Append(Server.HtmlEncode(request.ResultExecutedTime.ToString("yyyy-MM-dd hh:mm:ss.fff")));

            sb.Append("<p><h2>Response Code: ");
            sb.Append(request.ResponseHttpCode);
            sb.Append("</h2></p>");

            if (!string.IsNullOrEmpty(request.Exception))
            {
                sb.Append("<p><h2>Exception</h2><br/>");
                sb.Append("<textarea class=\"form-control\" rows=\"10\" style=\"margin-right:20px;\">");
                sb.Append(Server.HtmlEncode(request.Exception));
                sb.Append("</textarea></p>");
            }

            sb.Append("<p><h2>Response Headers</h2><br/>");
            sb.Append("<table class=\"table table-hover\"><thead><tr><td>Key</td><td>Value</td></tr></thead><tbody>");
            foreach(var pair in request.ResponseHeaders)
            {
                sb.Append("<tr><td>");
                sb.Append(Server.HtmlEncode(pair.Key));
                sb.Append("</td><td>");
                sb.Append(Server.HtmlEncode(pair.Value));
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
            sb.Append("</p>");

            sb.Append("<p><h2>Response Body:&nbsp;");
            sb.Append(request.ResponseBodyLength);
            sb.Append(" bytes</h2><br/>");
            sb.Append("<textarea class=\"form-control\" rows=\"10\" style=\"margin-right:20px;\">");
            if (request.ResponseBody != null)
            {
                sb.Append(Server.HtmlEncode(Encoding.UTF8.GetString(request.ResponseBody)));
            }
            sb.Append("</textarea></p>");

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
