using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Text
{
    public static class KnownStringComparers
    {
        private static readonly IComparer<string> _chineseCulture = new CultureAwareComparer(new CultureInfo("zh-cn"), false);
        private static readonly IComparer<string> _chineseCultureIgnoreCase = new CultureAwareComparer(new CultureInfo("zh-cn"), true);

        public static IComparer<string> ChineseCulture
        {
            get
            {
                Contract.Ensures(Contract.Result<IComparer<string>>() != null);
                return _chineseCulture;
            }
        }

        public static IComparer<string> ChineseCultureIgnoreCase
        {
            get
            {
                Contract.Ensures(Contract.Result<IComparer<string>>() != null);
                return _chineseCultureIgnoreCase;
            }
        }

    }
}
