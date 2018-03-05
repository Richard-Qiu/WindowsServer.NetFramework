using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Text
{
    public static class StringUtility
    {
        // ToSBC() is the opposite of ToDBC(), but it is rarely used and encourage NOT to be used.
        // So we comment out ToSBC().
        //public static string ToSBC(string input)
        //{
        //    var c = input.ToCharArray();
        //    for (int i = 0; i < c.Length; i++)
        //    {
        //        if (c[i] == 32)
        //        {
        //            c[i] = (char)12288;
        //        }
        //        else if (c[i] < 127)
        //        {
        //            c[i] = (char)(c[i] + 65248);
        //        }
        //    }
        //    return new string(c);
        //}

        public static string ToDBC(string input)
        {
            var c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                }
                else if ((c[i] > 65280) && (c[i] < 65375))
                {
                    c[i] = (char)(c[i] - 65248);
                }
            }
            return new string(c);
        }

        public static string RemoveWhiteSpaces(string input)
        {
            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                if (!char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }



        private static int LowerOfThree(int first, int second, int third)
        {
            int min = Math.Min(first, second);
            return Math.Min(min, third);
        }

        public static int CalculateLevenshteinDistance(string str1, string str2)
        {
            int[,] matrix;
            int n = str1.Length;
            int m = str2.Length;

            int temp = 0;
            char ch1;
            char ch2;
            int i = 0;
            int j = 0;
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {

                return n;
            }
            matrix = new int[n + 1, m + 1];

            for (i = 0; i <= n; i++)
            {
                matrix[i, 0] = i;
            }

            for (j = 0; j <= m; j++)
            {
                matrix[0, j] = j;
            }

            for (i = 1; i <= n; i++)
            {
                ch1 = str1[i - 1];
                for (j = 1; j <= m; j++)
                {
                    ch2 = str2[j - 1];
                    if (ch1.Equals(ch2))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = 1;
                    }

                    var min = Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1);
                    matrix[i, j] = Math.Min(min, matrix[i - 1, j - 1] + temp);
                }
            }

            return matrix[n, m];
        }

        public static double CalculateLevenshteinDistancePercent(string str1, string str2)
        {
            int val = CalculateLevenshteinDistance(str1, str2);
            return 1 - (double)val / Math.Max(str1.Length, str2.Length);
        }
    }
}
