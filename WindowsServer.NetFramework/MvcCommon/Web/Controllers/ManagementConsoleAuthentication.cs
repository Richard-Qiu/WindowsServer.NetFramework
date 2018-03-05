using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using WindowsServer.Configuration;

namespace WindowsServer.Web.Controllers
{
    internal static class ManagementConsoleAuthentication
    {
        private const string TokenCookieName = "mc_token";
        private static string _managementConsoleMd5Pin;

        static ManagementConsoleAuthentication()
        {
            _managementConsoleMd5Pin = ConfigurationCenter.Global["ManagementConsoleMd5Pin"] ?? "92F8A23C6B368AC65562A1B952BAF37E";
        }

        public static HttpCookie GenerateTokenCookie(string pin)
        {
            using (var md5 = MD5CryptoServiceProvider.Create())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(pin));
                var md5Pin = BitConverter.ToString(bytes).Replace("-", "");

                if (string.CompareOrdinal(md5Pin, _managementConsoleMd5Pin) != 0)
                {
                    throw new Exception("Pin does not match.");
                }
            }

            var cookie = new HttpCookie(TokenCookieName, GenerateToken())
            {
                Path = UrlUtility.Combine(HostingEnvironment.ApplicationVirtualPath, "ManagementConsole"),
            };
            return cookie;
        }

        public static void VerifyTokenCookie(Controller controller)
        {
            var cookie = HttpContext.Current.Request.Cookies[TokenCookieName];
            var token = GenerateToken();
            if ((cookie == null) || (cookie.Value != token))
            {
                HttpContext.Current.Response.Redirect(controller.Url.Action("SignIn", "ManagementConsole"), true);
            }
        }

        public static string GenerateToken()
        {
            var utcNow = DateTime.UtcNow;
            var hour = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0);

            var s = _managementConsoleMd5Pin + hour.ToString("yyyyMMddHHmmss");
            using (var md5 = MD5CryptoServiceProvider.Create())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(s));
                var md5Pin = BitConverter.ToString(bytes).Replace("-", "");
                return md5Pin;
            }
        }
    }
}
