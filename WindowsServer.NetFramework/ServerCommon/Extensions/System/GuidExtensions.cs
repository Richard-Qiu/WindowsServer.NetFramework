using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class GuidExtensions
    {
        public static bool IsEmpty(this Guid guid)
        {
            return guid == Guid.Empty;
        }
    }
}
