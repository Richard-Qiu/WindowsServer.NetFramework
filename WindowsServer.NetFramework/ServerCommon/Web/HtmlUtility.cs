using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Web
{
    public static class HtmlUtility
    {
        public static string HtmlEncode(string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

    }
}
