using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Log;
using WindowsServer.Web;

namespace WindowsServer.Middlewares
{
    public class RequestTrackerMiddleware
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private Func<IDictionary<string, object>, Task> _next;

        private static Dictionary<Guid, RequestTrackerData> _requestMap = new Dictionary<Guid, RequestTrackerData>();

        public RequestTrackerMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            this._next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var id = Guid.NewGuid();

            BeforeProcessing(id, environment);

            // Hijack the response stream
            var responseBody = EnvironmentHelper.GetResponseBody(environment);
            var hijackBody = new RequestTrackerHijackStream(responseBody, 128 * 1024); // 128 KB
            EnvironmentHelper.SetResponseBody(environment, hijackBody);

            try
            {
                var path = EnvironmentHelper.GetRequestPath(environment);
                if (string.Compare(path, "/RequestTracker/B", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var keyValues = EnvironmentHelper.ParseRequestKeyValues(environment);
                    BrowseRequests(
                        environment,
                        keyValues.ContainsKey("count") ? (int?)int.Parse(keyValues["count"]) : null,
                        keyValues.ContainsKey("ipAddress") ? keyValues["ipAddress"] : null,
                        keyValues.ContainsKey("userAgent") ? keyValues["userAgent"] : null,
                        keyValues.ContainsKey("accessToken") ? keyValues["accessToken"] : null
                    );
                }
                else if (path.StartsWith("/RequestTracker/BR/", StringComparison.OrdinalIgnoreCase))
                {
                    var idString = path.Substring("/RequestTracker/BR/".Length);
                    BrowseRequest(environment, Guid.Parse(idString));
                }
                else
                {
                    await _next(environment);
                }
            }
            catch(Exception ex)
            {
                // Restore the original response stream
                EnvironmentHelper.SetResponseBody(environment, responseBody);

                AfterProcessing(id, environment, hijackBody, ex);
                throw;
            }
            // Restore the original response stream
            EnvironmentHelper.SetResponseBody(environment, responseBody);

            var internalException = EnvironmentHelper.GetApplicationInternalException(environment);
            if (internalException != null)
            {
                AfterProcessing(id, environment, hijackBody, internalException);
            }
            else
            {
                AfterProcessing(id, environment, hijackBody, null);
            }
        }

        private void BeforeProcessing(Guid id, IDictionary<string, object> environment)
        {
            var data = new RequestTrackerData()
            {
                Id = id,
                Environment = environment,
                Method = EnvironmentHelper.GetRequestMethod(environment),
                Url = EnvironmentHelper.GetRequestPathWithQueryString(environment),
                UserAgent = EnvironmentHelper.GetRequestUserAgent(environment),
                IpAddress = EnvironmentHelper.GetServerRemoteIpAddress(environment, string.Empty),
                RequestHeaders = new Dictionary<string, string>(),
                RequestBodyLength = 0,
                RequestBody = null,
                ResponseHeaders = new Dictionary<string, string>(),
                ResponseBody = null,
                ProcessingTime = DateTime.UtcNow,
            };

            var headers = EnvironmentHelper.GetRequestHeaders(environment);
            foreach (var key in headers.Keys)
            {
                data.RequestHeaders[key] = string.Join(", ", headers[key]);
            }

            var stream = EnvironmentHelper.GetRequestBody(environment);
            if ((stream == null) || (stream == Stream.Null))
            {
                data.RequestBodyLength = 0;
                data.RequestBody = new byte[0];
            }
            else if (stream.CanSeek)
            {
                long previousPosition = stream.Position;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    memoryStream.Flush();

                    data.RequestBodyLength = memoryStream.Length;
                    data.RequestBody = new byte[memoryStream.Length];
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.Read(data.RequestBody, 0, data.RequestBody.Length);
                }
                stream.Seek(previousPosition, SeekOrigin.Begin);
            }
            else
            {
                data.RequestBody = Encoding.UTF8.GetBytes("The request input stream cannot seek.");
            }

            lock (_requestMap)
            {
                _requestMap.Add(data.Id, data);
            }
        }

        private void AfterProcessing(Guid id, IDictionary<string, object> environment, RequestTrackerHijackStream hijackStream, Exception exception)
        {
            RequestTrackerData data;
            lock (_requestMap)
            {
                if (!_requestMap.TryGetValue(id, out data))
                {
                    _logger.Error("Cannot find the RequestTrackerData in _requestMap. Id:" + id.ToString("N"));
                    return;
                }
            }

            DateTime utcNow = DateTime.UtcNow;
            data.ProcessedTime = utcNow;

            if (exception != null)
            {
                data.Exception = exception.ToString();
            }
            else
            {
                data.Exception = string.Empty;

                data.ResponseHttpCode = EnvironmentHelper.GetResponseStatusCode(environment, 0);
                var headers = EnvironmentHelper.GetResponseHeaders(environment);
                foreach (var pair in headers)
                {
                    data.ResponseHeaders[pair.Key] = string.Join(", ", pair.Value);
                }

                var body = EnvironmentHelper.GetResponseBody(environment);
                if ((body == null) || (body == Stream.Null))
                {
                    data.ResponseBody = new byte[0];
                    data.ResponseBodyLength = data.ResponseBody.Length;
                }
                else
                {
                    data.ResponseBody = hijackStream.GenerateHijackSnapshot();
                    data.ResponseBodyLength = hijackStream.TotalLength;
                }
            }

        }




        internal static List<RequestTrackerData> GenerateSnapshot(int? count, string ipAddress, string userAgent, string accessToken)
        {
            var lives = new List<RequestTrackerData>();
            lock (_requestMap)
            {
                var query = _requestMap.Values.AsQueryable();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    query = query.Where(r => r.IpAddress == ipAddress);
                }
                if (!string.IsNullOrEmpty(userAgent))
                {
                    query = query.Where(r => r.UserAgent == userAgent);
                }
                if (!string.IsNullOrEmpty(accessToken))
                {
                    query = query.Where(r => r.AccessToken == accessToken);
                }

                query = query.OrderByDescending(r => r.ProcessingTime);

                if (count.HasValue)
                {
                    query = query.Take(count.Value);
                }

                return query.ToList();
            }
        }

        internal static RequestTrackerData GetRequestData(Guid requestId)
        {
            lock (_requestMap)
            {
                RequestTrackerData data;
                _requestMap.TryGetValue(requestId, out data);
                return data;
            }
        }

        public void BrowseRequests(IDictionary<string, object> environment, int? count, string ipAddress, string userAgent, string accessToken)
        {
            if (EnvironmentHelper.GetRequestMethod(environment) == "GET")
            {
                if (!count.HasValue)
                {
                    count = 100;
                }
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = EnvironmentHelper.GetServerRemoteIpAddress(environment, string.Empty);
                }
            }

            var requests = GenerateSnapshot(count, ipAddress, userAgent, accessToken);

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
                sb.Append(HtmlUtility.HtmlEncode("../RequestTracker/BR/" + request.Id.ToString("N")));
                sb.Append("\">Detail</a>");
                sb.Append("</td><td>");
                sb.Append(HtmlUtility.HtmlEncode(request.ProcessingTime.ToString("yyyy-MM-dd hh:mm:ss.fff")));
                sb.Append("</td><td>");
                if (request.ProcessedTime == DateTime.MinValue)
                {
                    sb.Append("Processing");
                }
                else
                {
                    sb.Append((request.ProcessedTime - request.ProcessingTime).TotalMilliseconds);
                }
                sb.Append("</td><td>");
                sb.Append(HtmlUtility.HtmlEncode(request.IpAddress));
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
                sb.Append(HtmlUtility.HtmlEncode(request.Url));
                sb.Append("</td><td>");
                sb.Append(HtmlUtility.HtmlEncode(request.UserAgent));
                sb.Append("</td><td>");
                sb.Append(HtmlUtility.HtmlEncode(request.AccessToken));
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table>");

            RenderBootstrapHtmlPostfix(sb);

            EnvironmentHelper.SetResponse(environment, 200, "text/html", sb.ToString());
        }

        public void BrowseRequest(IDictionary<string, object> environment, Guid id)
        {
            var request = GetRequestData(id);
            if (request == null)
            {
                EnvironmentHelper.SetResponse(environment, 404, "text/html", "Cannot find the request.");
            }

            var sb = new StringBuilder(4096);
            RenderBootstrapHtmlPrefix(sb, request.Method + ": " + request.Url);
            sb.Append("<p><h1>");
            sb.Append(HtmlUtility.HtmlEncode(request.Method));
            sb.Append(": ");
            sb.Append(HtmlUtility.HtmlEncode(request.Url));
            sb.Append("&nbsp;&nbsp;&nbsp;<small>");
            sb.Append(request.Id.ToString("N"));
            sb.Append("</small>");
            sb.Append("</h1></p>");

            sb.Append("<p><h2>UserAgent:&nbsp;&nbsp;&nbsp;");
            sb.Append(HtmlUtility.HtmlEncode(request.UserAgent));
            sb.Append("</h2></p>");

            sb.Append("<p><h2>IP Address:&nbsp;&nbsp;&nbsp;");
            sb.Append(HtmlUtility.HtmlEncode(request.IpAddress));
            sb.Append("</h2></p>");

            sb.Append("<p><h2>Request Headers</h2><br/>");
            sb.Append("<table class=\"table table-hover\"><thead><tr><td>Key</td><td>Value</td></tr></thead><tbody>");
            foreach (var pair in request.RequestHeaders)
            {
                sb.Append("<tr><td>");
                sb.Append(HtmlUtility.HtmlEncode(pair.Key));
                sb.Append("</td><td>");
                sb.Append(HtmlUtility.HtmlEncode(pair.Value));
                sb.Append("</td></tr>");
            }
            sb.Append("</tbody></table>");
            sb.Append("</p>");

            sb.Append("<p><h2>Request Body:&nbsp;");
            sb.Append(request.RequestBodyLength);
            sb.Append(" bytes</h2><br/>");
            sb.Append("<textarea class=\"form-control\" rows=\"10\" style=\"margin-right:20px;\">");
            sb.Append(HtmlUtility.HtmlEncode(Encoding.UTF8.GetString(request.RequestBody)));
            sb.Append("</textarea></p>");

            sb.Append("<p><h2>Time:&nbsp;");
            sb.Append((request.ProcessedTime - request.ProcessingTime).TotalMilliseconds);
            sb.Append("ms</h2><br/>ProcessingTime(UTC):&nbsp;");
            sb.Append(HtmlUtility.HtmlEncode(request.ProcessingTime.ToString("yyyy-MM-dd hh:mm:ss.fff")));
            sb.Append("<br/>ProcessedTime(UTC):&nbsp;");
            sb.Append(HtmlUtility.HtmlEncode(request.ProcessedTime.ToString("yyyy-MM-dd hh:mm:ss.fff")));

            sb.Append("<p><h2>Response Code: ");
            sb.Append(request.ResponseHttpCode);
            sb.Append("</h2></p>");

            if (!string.IsNullOrEmpty(request.Exception))
            {
                sb.Append("<p><h2>Exception</h2><br/>");
                sb.Append("<textarea class=\"form-control\" rows=\"10\" style=\"margin-right:20px;\">");
                sb.Append(HtmlUtility.HtmlEncode(request.Exception));
                sb.Append("</textarea></p>");
            }

            sb.Append("<p><h2>Response Headers</h2><br/>");
            sb.Append("<table class=\"table table-hover\"><thead><tr><td>Key</td><td>Value</td></tr></thead><tbody>");
            foreach (var pair in request.ResponseHeaders)
            {
                sb.Append("<tr><td>");
                sb.Append(HtmlUtility.HtmlEncode(pair.Key));
                sb.Append("</td><td>");
                sb.Append(HtmlUtility.HtmlEncode(pair.Value));
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
                sb.Append(HtmlUtility.HtmlEncode(Encoding.UTF8.GetString(request.ResponseBody)));
            }
            sb.Append("</textarea></p>");

            RenderBootstrapHtmlPostfix(sb);

            EnvironmentHelper.SetResponse(environment, 200, "text/html", sb.ToString());
        }

        private void RenderBootstrapHtmlPrefix(StringBuilder html, string title)
        {
            html.Append("<!DOCTYPE html><html lang=\"en\"><head><title>");
            html.Append(HtmlUtility.HtmlEncode(title));
            html.Append("</title><link rel=\"stylesheet\" href=\"http://netdna.bootstrapcdn.com/bootstrap/3.0.3/css/bootstrap.min.css\"><link rel=\"stylesheet\" href=\"http://netdna.bootstrapcdn.com/bootstrap/3.0.3/css/bootstrap-theme.min.css\"></head><body>");
        }

        private void RenderBootstrapHtmlPostfix(StringBuilder html)
        {
            html.Append("<script src=\"https://code.jquery.com/jquery.js\"></script><script src=\"http://netdna.bootstrapcdn.com/bootstrap/3.0.3/js/bootstrap.min.js\"></script></body></html>");
        }

    }
}
