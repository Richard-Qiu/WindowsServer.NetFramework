using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Web;

namespace WindowsServer
{
    public static class EnvironmentHelper
    {
        public static readonly string OwinRequestBodyKey = "owin.RequestBody";
        public static readonly string OwinRequestHeadersKey = "owin.RequestHeaders";
        public static readonly string OwinRequestMethodKey = "owin.RequestMethod";
        public static readonly string OwinRequestPathKey = "owin.RequestPath";
        public static readonly string OwinRequestPathBaseKey = "owin.RequestPathBase";
        public static readonly string OwinRequestProtocolKey = "owin.RequestProtocol";
        public static readonly string OwinRequestQueryStringKey = "owin.RequestQueryString";
        public static readonly string OwinRequestSchemeKey = "owin.RequestScheme";
        public static readonly string OwinResponseBodyKey = "owin.ResponseBody";
        public static readonly string OwinResponseHeadersKey = "owin.ResponseHeaders";
        public static readonly string OwinResponseStatusCodeKey = "owin.ResponseStatusCode";
        public static readonly string OwinResponseReasonPhraseKey = "owin.ResponseReasonPhrase";
        public static readonly string OwinResponseProtocolKey = "owin.ResponseProtocol";

        public static readonly string ServerRemoteIpAddressKey = "server.RemoteIpAddress";

        public static readonly string ApplicationInternalExceptionKey = "application.InternalException";

        public static Stream GetRequestBody(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinRequestBodyKey, out o))
            {
                return o as Stream;
            }
            return Stream.Null;
        }

        public static IDictionary<string, string[]> GetRequestHeaders(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinRequestHeadersKey, out o))
            {
                return o as IDictionary<string, string[]>;
            }
            return new Dictionary<string, string[]>();
        }

        public static string GetRequestMethod(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinRequestMethodKey, out o))
            {
                return o as string;
            }
            return string.Empty;
        }

        public static string GetRequestPath(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinRequestPathKey, out o))
            {
                return o as string;
            }
            return string.Empty;
        }

        public static string GetRequestPathBase(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinRequestPathBaseKey, out o))
            {
                return o as string;
            }
            return string.Empty;
        }

        public static string GetRequestProtocol(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinRequestProtocolKey, out o))
            {
                return o as string;
            }
            return string.Empty;
        }

        public static string GetRequestQueryString(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinRequestQueryStringKey, out o))
            {
                return o as string;
            }
            return string.Empty;
        }

        public static string GetRequestScheme(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinRequestSchemeKey, out o))
            {
                return o as string;
            }
            return string.Empty;
        }

        public static string GetRequestHeaderValue(IDictionary<string, object> environment, string headerKey)
        {
            var headers = GetRequestHeaders(environment);
            string v = null;
            if (headers != null)
            {
                string[] s;
                if (headers.TryGetValue(headerKey, out s))
                {
                    v = string.Join(", ", s);
                }
            }
            return v;
        }

        public static string GetRequestUserAgent(IDictionary<string, object> environment)
        {
            return GetRequestHeaderValue(environment, "User-Agent") ?? string.Empty;
        }

        public static string GetRequestContentType(IDictionary<string, object> environment)
        {
            return GetRequestHeaderValue(environment, "Content-Type") ?? string.Empty;
        }

        public static string GetRequestPathWithQueryString(IDictionary<string, object> environment)
        {
            var path = GetRequestPathBase(environment) + GetRequestPath(environment);
            var queryString = GetRequestQueryString(environment);
            if (string.IsNullOrEmpty(queryString))
            {
                return path;
            }
            else
            {
                return path + "?" + queryString;
            }
        }

        public static Dictionary<string, string> ParseRequestKeyValues(IDictionary<string, object> environment)
        {
            var result = new Dictionary<string, string>();

            // Parse body
            if (GetRequestContentType(environment).ToLower().Contains("application/x-www-form-urlencoded"))
            {
                var body = GetRequestBody(environment);
                if ((body != null) && (body != Stream.Null))
                {
                    var reader = new StreamReader(body);
                    var s = reader.ReadToEnd();

                    foreach (var pair in s.Split(new char[] {'&'}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var assignmentIndex = pair.IndexOf('=');
                        if (assignmentIndex > 0)
                        {
                            var key = pair.Substring(0, assignmentIndex);
                            var value = (assignmentIndex < (pair.Length - 1)) ? pair.Substring(assignmentIndex + 1) : string.Empty;
                            result[UrlUtility.UrlDecode(key)] = UrlUtility.UrlDecode(value);
                        }
                    }
                }
            }

            // Parse query string
            var queryString = GetRequestQueryString(environment);
            if (!string.IsNullOrEmpty(queryString))
            {
                foreach (var pair in queryString.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var assignmentIndex = pair.IndexOf('=');
                    if (assignmentIndex > 0)
                    {
                        var key = pair.Substring(0, assignmentIndex);
                        var value = (assignmentIndex < (pair.Length - 1)) ? pair.Substring(assignmentIndex + 1) : string.Empty;
                        result[UrlUtility.UrlDecode(key)] = UrlUtility.UrlDecode(value);
                    }
                }
            }

            return result;
        }

        public static Stream GetResponseBody(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinResponseBodyKey, out o))
            {
                return o as Stream;
            }
            return Stream.Null;
        }

        public static void SetResponseBody(IDictionary<string, object> environment, Stream body)
        {
            environment[OwinResponseBodyKey] = body;
        }

        public static void WriteToResponseBody(IDictionary<string, object> environment, byte[] body)
        {
            GetResponseBody(environment).Write(body, 0, body.Length);
        }

        public static IDictionary<string, string[]> GetResponseHeaders(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(OwinResponseHeadersKey, out o))
            {
                return o as IDictionary<string, string[]>;
            }
            return null;
        }

        public static void SetResponseHeader(IDictionary<string, object> environment, string headerKey, string[] headerValue)
        {
            var headers = GetResponseHeaders(environment);
            headers[headerKey] = headerValue;
        }

        public static void SetResponseContentType(IDictionary<string, object> environment, string contentType)
        {
            SetResponseHeader(environment, "Content-Type", new string[1] { contentType });
        }

        public static void SetResponseContentLength(IDictionary<string, object> environment, long contentLength)
        {
            SetResponseHeader(environment, "Content-Length", new string[1] { contentLength.ToString() });
        }

        public static int GetResponseStatusCode(IDictionary<string, object> environment, int defaultValue)
        {
            object o;
            if (environment.TryGetValue(OwinResponseStatusCodeKey, out o))
            {
                return (int)o;
            }
            return defaultValue;
        }

        public static void SetResponseStatusCode(IDictionary<string, object> environment, int statusCode)
        {
            environment[OwinResponseStatusCodeKey] = statusCode;
        }

        public static string GetResponseReasonPhrase(IDictionary<string, object> environment, string defaultValue)
        {
            object o;
            if (environment.TryGetValue(OwinResponseReasonPhraseKey, out o))
            {
                return o as string;
            }
            return defaultValue;
        }

        public static string GetResponseProtocol(IDictionary<string, object> environment, string defaultValue)
        {
            object o;
            if (environment.TryGetValue(OwinResponseProtocolKey, out o))
            {
                return o as string;
            }
            return defaultValue;
        }

        public static void SetResponse(IDictionary<string, object> environment, int statusCode, string contentType, string body)
        {
            SetResponseStatusCode(environment, statusCode);
            if (!contentType.Contains("charset"))
            {
                contentType += "; charset=utf-8";
            }
            SetResponseContentType(environment, contentType);
            var bytes = Encoding.UTF8.GetBytes(body);
            //SetResponseContentLength(environment, bytes.Length);
            WriteToResponseBody(environment, bytes);
        }

        public static string GetServerRemoteIpAddress(IDictionary<string, object> environment, string defaultValue)
        {
            object o;
            if (environment.TryGetValue(ServerRemoteIpAddressKey, out o))
            {
                return o as string;
            }
            return defaultValue;
        }

        public static Exception GetApplicationInternalException(IDictionary<string, object> environment)
        {
            object o;
            if (environment.TryGetValue(ApplicationInternalExceptionKey, out o))
            {
                return o as Exception;
            }
            return null;
        }

        public static void SetApplicationInternalException(IDictionary<string, object> environment, Exception exception)
        {
            environment[ApplicationInternalExceptionKey] = exception;
        }
    }
}
