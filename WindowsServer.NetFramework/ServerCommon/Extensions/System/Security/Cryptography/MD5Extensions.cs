using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Security.Cryptography
{
    public static class MD5Extensions
    {
        public static byte[] ComputeHash(this MD5 md5, string s)
        {
            return md5.ComputeHash(Encoding.UTF8.GetBytes(s));
        }
    }
}
