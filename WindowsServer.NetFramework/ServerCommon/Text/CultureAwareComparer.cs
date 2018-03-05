using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Text
{
    [Serializable]
    internal sealed class CultureAwareComparer : IComparer<string>
    {
        private CompareInfo _compareInfo;
        private bool _ignoreCase;

        internal CultureAwareComparer(CultureInfo culture, bool ignoreCase)
        {
            _compareInfo = culture.CompareInfo;
            _ignoreCase = ignoreCase;
        }

        internal CultureAwareComparer(CompareInfo compareInfo, bool ignoreCase)
        {
            _compareInfo = compareInfo;
            _ignoreCase = ignoreCase;
        }

        public int Compare(string x, string y)
        {
            if (Object.ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return _compareInfo.Compare(x, y, _ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        }
    }

}
