using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WindowsServer.Web
{
    public static class UrlUtility
    {
        public static string Combine(string url1, string url2)
        {
            if (string.IsNullOrEmpty(url1))
            {
                return url2;
            }
            if (string.IsNullOrEmpty(url2))
            {
                return url1;
            }

            if (url2.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                || url2.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                return url2;
            }

            bool url1EndSlash = (url1[url1.Length - 1] == '/');
            bool url2StartSlash = (url2[0] == '/');
            if ((url1EndSlash && !url2StartSlash) || (!url1EndSlash && url2StartSlash))
            {
                return url1 + url2;
            }
            if (url1EndSlash && url2StartSlash)
            {
                return url1 + url2.Substring(1);
            }
            return url1 + "/" + url2;
        }

        public static string ConvertBase64ToUrlBase64(string base64String)
        {
            string s = base64String.Replace('+', '-').Replace('/', '_');
            return s.TrimEnd('=');
        }

        public static string ConvertUrlBase64ToBase64(string urlBase64String)
        {
            string s = urlBase64String.Replace('-', '+').Replace('_', '/');
            if ((s.Length % 4) == 0)
            {
                return s;
            }
            return s + new String('=', 4 - (s.Length % 4));
        }

        public static string Format(string format, params object[] args)
        {
            string[] ss = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                object arg = args[i];
                string s;
                if (arg is DateTime)
                {
                    s = ((DateTime)arg).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (arg is Guid)
                {
                    s = ((Guid)arg).ToString("N");
                }
                else
                {
                    s = arg.ToString();
                }
                ss[i] = HttpUtility.UrlEncode(s, Encoding.UTF8);
            }
            return string.Format(format, ss);
        }

        public static string UrlEncode(string s)
        {
            return HttpUtility.UrlEncode(s);
        }

        public static string UrlDecode(string s)
        {
            return HttpUtility.UrlDecode(s);
        }
    }
}
