using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Encryption
{
    public static class EncryptedBase64
    {
        private const char delta = (char)('a' - 'A');

        public static string ToEncryptedBase64String(byte[] inArray)
        {
            var base64 = Convert.ToBase64String(inArray);
            return Exchange(base64);
        }

        public static byte[] FromEncryptedBase64String(string encryptedBase64)
        {
            var b = Exchange(encryptedBase64);
            return Convert.FromBase64String(b);
        }

        private static string Exchange(string s)
        {
            var result = new StringBuilder();
            foreach (var c in s)
            {
                if ((c >= 'A') && (c <= 'Z'))
                {
                    result.Append((char)(c + delta));
                }
                else if ((c >= 'a') && (c <= 'z'))
                {
                    result.Append((char)(c - delta));
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }
    }
}
