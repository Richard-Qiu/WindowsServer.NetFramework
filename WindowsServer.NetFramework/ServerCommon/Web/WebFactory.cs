using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Web
{
    public static class WebFactory
    {
        private static readonly string s_userAgent;

        static WebFactory()
        {
            s_userAgent = "WinSvr " + AppDomain.CurrentDomain.FriendlyName;
            s_userAgent = s_userAgent.Replace(',', '_');
            s_userAgent = s_userAgent.Replace(':', '_');
            s_userAgent = s_userAgent.Replace('/', '_');
        }

        public static HttpClient CreateHttpClient()
        {
            return CreateHttpClient(string.Empty);
        }

        public static HttpClient CreateHttpClient(string additionalUserAgent)
        {
            var client = new HttpClient();
            var userAgent = string.IsNullOrEmpty(additionalUserAgent) ? s_userAgent : s_userAgent + " " + additionalUserAgent;
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            return client;
        }

        public static WebClient CreateWebClient()
        {
            return CreateWebClient(string.Empty);
        }

        public static WebClient CreateWebClient(string additionalUserAgent)
        {
            var client = new WebClient();
            var userAgent = string.IsNullOrEmpty(additionalUserAgent) ? s_userAgent : s_userAgent + " " + additionalUserAgent;
            client.Headers[HttpRequestHeader.UserAgent] = userAgent;
            return client;
        }

        public static HttpWebRequest CreateHttpWebRequest(string requestUriString)
        {
            return CreateHttpWebRequest(requestUriString, string.Empty);
        }

        public static HttpWebRequest CreateHttpWebRequest(string requestUriString, string additionalUserAgent)
        {
            var request = WebRequest.CreateHttp(requestUriString);
            var userAgent = string.IsNullOrEmpty(additionalUserAgent) ? s_userAgent : s_userAgent + " " + additionalUserAgent;
            request.UserAgent = userAgent;
            return request;
        }

        public static HttpWebRequest CreateHttpWebRequest(Uri requestUri)
        {
            return CreateHttpWebRequest(requestUri, string.Empty);
        }

        public static HttpWebRequest CreateHttpWebRequest(Uri requestUri, string additionalUserAgent)
        {
            var request = WebRequest.CreateHttp(requestUri);
            var userAgent = string.IsNullOrEmpty(additionalUserAgent) ? s_userAgent : s_userAgent + " " + additionalUserAgent;
            request.UserAgent = userAgent;
            return request;
        }

    }
}
