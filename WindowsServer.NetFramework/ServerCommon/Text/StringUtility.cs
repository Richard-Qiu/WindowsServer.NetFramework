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



        public static int DivRem(int a, int b)
        {
            int result = 0;
            int num3 = Math.DivRem(a, b, out result);
            if (result > 0)
            {
                num3++;
            }
            if (num3 > 0)
            {
                return num3;
            }
            return 1;
        }

        public static string GetString(string[] strArray, int len)
        {
            int num = 0;
            StringBuilder builder = new StringBuilder();
            int byteCount = 1;
            for (int i = 0; i < strArray.Length; i++)
            {
                byteCount = Encoding.Default.GetByteCount(strArray[i].ToString());
                num += byteCount;
                if (num > len)
                {
                    break;
                }
                builder.Append(strArray[i]);
            }
            if (builder.Length > 0)
            {
                return builder.ToString();
            }
            return string.Empty;
        }

        public static bool IsNullorEmpty(string text)
        {
            if (text != null)
            {
                return (text.Trim() == string.Empty);
            }
            return true;
        }

        public static string LeftB(string strValue, int len)
        {
            if (!string.IsNullOrEmpty(strValue) && (len > 0))
            {
                return GetString(toStringArray(strValue), len);
            }
            return string.Empty;
        }

        public static string LeftB(string strValue, int len, string strFill)
        {
            return LeftB(strValue, len, strFill, true);
        }

        public static string LeftB(string strValue, int len, string strFill, bool isfillleft)
        {
            string s = string.Empty;
            if (!string.IsNullOrEmpty(strValue) && (len > 0))
            {
                s = GetString(toStringArray(strValue), len);
            }
            if (!string.IsNullOrEmpty(strFill))
            {
                while (Encoding.Default.GetByteCount(s) < len)
                {
                    if (isfillleft)
                    {
                        s = s + strFill;
                    }
                    else
                    {
                        s = strFill + s;
                    }
                }
            }
            return s;
        }

        public static List<string> LeftList(string strValue, int len, string strFill)
        {
            List<string> list = new List<string>();
            if (len > 0)
            {
                if (string.IsNullOrEmpty(strValue) || (len <= 0))
                {
                    return list;
                }
                string[] strArray = toStringArray(strValue);
                int byteCount = 1;
                int num2 = 0;
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < strArray.Length; i++)
                {
                    byteCount = Encoding.Default.GetByteCount(strArray[i].ToString());
                    num2 += byteCount;
                    if (num2 > len)
                    {
                        if (!string.IsNullOrEmpty(strFill))
                        {
                            while (Encoding.Default.GetByteCount(builder.ToString()) < len)
                            {
                                builder.Append(strFill);
                            }
                        }
                        if (builder.Length > 0)
                        {
                            list.Add(builder.ToString());
                        }
                        builder = new StringBuilder();
                        num2 = byteCount;
                    }
                    builder.Append(strArray[i]);
                }
                if (!string.IsNullOrEmpty(strFill))
                {
                    while (Encoding.Default.GetByteCount(builder.ToString()) < len)
                    {
                        builder.Append(strFill);
                    }
                }
                list.Add(builder.ToString());
            }
            return list;
        }

        public static string SubString(string content, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }
            if (content.Length < (startIndex + 1))
            {
                return string.Empty;
            }
            if (content.Length < (startIndex + length))
            {
                return string.Empty;
            }
            return content.Substring(startIndex, length);
        }

        public static string[] toStringArray(string str)
        {
            string[] strArray = new string[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                strArray[i] = str.Substring(i, 1);
            }
            return strArray;
        }

        public static bool TryBool(object obj)
        {
            return TryBool(obj, false);
        }

        public static bool TryBool(object obj, bool defBl)
        {
            if ((obj == null) || (obj == DBNull.Value))
            {
                return defBl;
            }
            if (obj.Equals("0"))
            {
                return false;
            }
            return (obj.Equals("1") || bool.Parse(obj.ToString()));
        }

        public static byte[] TryByte(object obj)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                return (byte[])obj;
            }
            return null;
        }

        public static DateTime TryDateTime(object obj)
        {
            return TryDateTime(obj, DateTime.Now);
        }

        public static DateTime TryDateTime(object obj, DateTime defInt)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                DateTime result = defInt;
                if (DateTime.TryParse(obj.ToString(), out result))
                {
                    return result;
                }
            }
            return defInt;
        }

        public static DateTime? TryDateTime2(object obj)
        {
            DateTime time;
            if (((obj != null) && (obj != DBNull.Value)) && DateTime.TryParse(obj.ToString(), out time))
            {
                return new DateTime?(time);
            }
            return null;
        }

        public static decimal TryDec(object obj)
        {
            return TryDec(obj, 0M);
        }

        public static decimal TryDec(object obj, decimal defInt)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                decimal result = 0M;
                if (decimal.TryParse(obj.ToString(), out result))
                {
                    return result;
                }
            }
            return defInt;
        }

        public static double TryDouble(object obj)
        {
            return TryDouble(obj, 0.0);
        }

        public static double TryDouble(object obj, double defInt)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                double result = 0.0;
                if (double.TryParse(obj.ToString(), out result))
                {
                    return result;
                }
            }
            return defInt;
        }

        public static float TryFloat(object obj)
        {
            return TryFloat(obj, 0f);
        }

        public static float TryFloat(object obj, float defInt)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                float result = 0f;
                if (float.TryParse(obj.ToString(), out result))
                {
                    return result;
                }
            }
            return defInt;
        }

        public static int TryInt(object obj)
        {
            return TryInt(obj, 0);
        }

        public static int TryInt(object obj, int defInt)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                int result = 0;
                if (int.TryParse(obj.ToString(), out result))
                {
                    return result;
                }
            }
            return defInt;
        }

        public static short TryInt16(object obj)
        {
            return TryInt16(obj, 0);
        }

        public static short TryInt16(object obj, short defInt)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                short result = 0;
                if (short.TryParse(obj.ToString(), out result))
                {
                    return result;
                }
            }
            return defInt;
        }

        public static long TryInt64(object obj)
        {
            return TryInt64(obj, 0L);
        }

        public static long TryInt64(object obj, long defInt)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                long result = 0L;
                if (long.TryParse(obj.ToString(), out result))
                {
                    return result;
                }
            }
            return defInt;
        }

        public static short TryShort(object obj)
        {
            return TryShort(obj, 0);
        }

        public static short TryShort(object obj, short defInt)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                short result = 0;
                if (short.TryParse(obj.ToString(), out result))
                {
                    return result;
                }
            }
            return defInt;
        }

        public static string TryStr(object obj)
        {
            return TryStr(obj, string.Empty);
        }

        public static string TryStr(object obj, string defInt)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                return obj.ToString().Trim();
            }
            return defInt;
        }

        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
    }
}
