using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class ListExtensions
    {
        public static int RemoveRange<T>(this List<T> thisList, IEnumerable<T> collection)
        {
            int count = 0;
            foreach (var item in collection)
            {
                if (thisList.Remove(item))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
