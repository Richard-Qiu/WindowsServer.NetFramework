using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WindowsServer.Web
{
    public static class HtmlManifestUtility
    {
        public const string ManifestMimeType = "text/cache-manifest";

        public static byte[] GenerateManifestBytes(string applicationPath, IEnumerable<string> files)
        {
            return Encoding.UTF8.GetBytes(GenerateManifestString(applicationPath, files));
        }

        private static string GenerateManifestString(string applicationPath, IEnumerable<string> fileUrls)
        {
            StringBuilder sb = new StringBuilder(1024);
            foreach (var fileUrl in fileUrls)
            {
                sb.Append("\r\n");
                sb.Append(UrlUtility.Combine(applicationPath, fileUrl));
            }
            sb.Append("\r\n");

            string mainContent = sb.ToString();

            // Generate version
            Guid version = Guid.Empty;
            using (MD5 md5 = MD5.Create())
            {
                version = new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(mainContent)));
            }

            return "CACHE MANIFEST\r\n# VERSION " + version.ToString("N") + mainContent;
        }

    }
}
