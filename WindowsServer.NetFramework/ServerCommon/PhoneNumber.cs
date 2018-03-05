using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.Contracts;
using WindowsServer.Text;

namespace WindowsServer
{
    public enum PhoneNumberKind
    {
        MobilePhone = 0,
        FixedPhone = 1,
        SpecialPhoneNumber = 2,
    }

    /// <summary>
    /// Treat it as UINT64 type. The highest 4 bits represent the kind of phone number.
    /// Currently, we have three kinds: 0: mobile phone; 1: fixed phone; 2:special phone numbers.
    /// The next 12 bits(0-4096) represent the country code. The left 48 bits(0-281474976710656) are for local mobile phone.
    /// +--------+-------------------+---------------------------------------------------+
    /// |  Kind  |   Country Code    |              Local Mobile Phone Number            |
    /// +--------+-------------------+---------------------------------------------------+
    /// | 4 bits |     12 bits       |                     48 bits                       |
    /// +--------+-------------------+---------------------------------------------------+
    /// 
    /// For example, a Chinese phone number is 13818285868 (0x337A24F2C in hex format), and
    /// the country code is 86(0x56). Since we are using little-endian, the bytes are 0x2C, 0x4F, 0xA2, 0x37, 0x03, 0x00, 0x56, 0x00.
    /// The full list of country code can be found in http://www.kropla.com/dialcode.htm and
    /// http://zh.wikipedia.org/wiki/%E5%9B%BD%E9%99%85%E7%94%B5%E8%AF%9D%E5%8C%BA%E5%8F%B7%E5%88%97%E8%A1%A8 .
    /// So far, the “Mobile Satellite Service” code, such as +8818, +8819 will not be covered.
    /// If it is a fixed phone number, we reserve 10 bits for area code.The left 38 bits(0 - 274877906944)
    /// are for local fixed phone number.It has 11 valid digits, which is enough for China.Not sure
    /// if it is also enough for other countries ?
    /// +--------+-------------------+--------------+------------------------------------+
    /// |  Kind  |   Country Code    |   Area Code  |      Local Mobile Phone Number     |
    /// +--------+-------------------+--------------+------------------------------------+
    /// | 4 bits |     12 bits       |   10 bits    |              38 bits               |
    /// +--------+-------------------+--------------+------------------------------------+
    /// 
    /// Because it might be very difficult for client application to get the area code of a fixed phone
    /// number, the area code can be ignored, set it to 0.
    /// For example, If a Shanghai fixed phone number is 58888888(0x38292B8), it is encoded(area code
    /// is ignored) as 0xB8, 0x92, 0x82, 0x03, 0x00, 0x00, 0x56, 0x00.Or it is encoded (including
    /// area code) as 0xB8, 0x92, 0x82, 0x03, 0x40, 0x05, 0x56, 0x00.
    /// Full list of Chinese area codes:
    /// http://zh.wikipedia.org/wiki/%E4%B8%AD%E5%9B%BD%E5%A4%A7%E9%99%86%E5%9C%B0%E5%8C%BA%E7%94%B5%E8%AF%9D%E5%8C%BA%E5%8F%B7
    /// Special phone numbers:
    /// System: 0x2000000000000001
    /// </summary>
    public struct PhoneNumber : IComparable, IComparable<Guid>, IEquatable<PhoneNumber>
    {
        public static readonly PhoneNumber Empty = new PhoneNumber();

        private const long _chinaCountryCode = ((long)86) << 48;
        private static int[] s_internationalDialingCodesRaw = new int[] {
            1,7,
            20,27,28,30,31,32,33,34,36,39,40,41,43,44,45,46,47,48,49,51,52,53,54,55,56,57,58,60,61,62,63,64,65,66,81,82,83,84,86,89,90,91,92,93,94,95,98,
            210,211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,
            250,251,252,253,254,255,256,257,258,259,260,261,262,263,264,265,266,267,268,269,290,291,292,293,294,295,296,297,298,299,
            350,351,352,353,354,355,356,357,358,359,371,372,373,374,375,376,377,378,379,380,381,382,383,384,385,386,387,388,389,
            420,421,422,423,424,425,426,427,428,429,
            500,501,502,503,504,505,506,507,508,509,590,591,592,593,594,595,596,597,598,599,
            670,671,672,673,674,675,676,677,678,679,680,681,682,683,684,685,686,687,688,689,690,691,692,693,694,695,696,697,698,699,
            850,851,852,853,854,855,856,857,858,859,870,871,872,873,874,875,876,877,878,879,880,881,882,883,884,885,886,887,888,889,
            960,961,962,963,964,965,966,967,968,969,970,971,972,973,974,975,976,977,978,979,990,991,992,993,994,995,996,997,998,999,
            1242,1246,1264,1268,1284,1340,1345,1441,1473,1649,1664,1670,1671,1684,1721,1758,1767,1784,1787,1809,1829,1849,1868,1869,1876,1939,
        };
        private static bool[] s_internationalDialingCodesMap = new bool[10000]; // All country codes are less than 5 digits.

        static PhoneNumber()
        {
            foreach(int internationalDialingCode in s_internationalDialingCodesRaw)
            {
                s_internationalDialingCodesMap[internationalDialingCode] = true;
            }
        }

        private long _number;
        //private long _ext;

        public PhoneNumberKind Kind
        {
            get
            {
                return (PhoneNumberKind)(_number >> 60);
            }
        }

        public int CountryCode
        {
            get
            {
                return (int)((_number >> 48) & 0xFFF);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (_number == 0);
            }
        }

        public long MobilePhone
        {
            get
            {
                return (_number & 0xFFFFFFFFFFFF);
            }
        }

        public int AreaCode
        {
            get
            {
                return (int)((_number >> 38) & 0x3FF);
            }
        }

        public long FixedPhone
        {
            get
            {
                return _number & 0x3FFFFFFFFF;
            }
        }

        public long RawValue
        {
            get
            {
                return _number;
            }
        }

        private enum PhoneNumberParseThrowStyle
        {
            None = 0,
            All = 1,
            AllButOverflow = 2
        }

        public PhoneNumber(long rawValue)
        {
            _number = rawValue;
        }

        public PhoneNumber(string phoneNumberString)
        {
            _number = PhoneNumber.Parse(phoneNumberString)._number;
        }

        public PhoneNumber(int countryCode, long mobilePhone)
        {
            _number = (((long)countryCode) << 48) | mobilePhone;
        }

        private struct PhoneNumberResult
        {
            internal PhoneNumber ParsedPhoneNumber;
            internal string _failureMessage;
            private Exception _innerException;
            private PhoneNumberParseThrowStyle _throwStyle;

            internal void Init(PhoneNumberParseThrowStyle canThrow)
            {
                ParsedPhoneNumber = PhoneNumber.Empty;
                _throwStyle = canThrow;
            }

            internal void SetFailure(Exception exception)
            {
                SetFailure(null, exception);
            }

            internal void SetFailure(string failureMessage)
            {
                SetFailure(failureMessage, null);
            }

            internal void SetFailure(string failureMessage, Exception innerException)
            {
                _failureMessage = failureMessage;
                _innerException = innerException;
                if (_throwStyle != PhoneNumberParseThrowStyle.None)
                {
                    throw GetPhoneNumberParseException();
                }
            }

            internal Exception GetPhoneNumberParseException()
            {
                return new Exception(_failureMessage, _innerException);
            }
        }

        public static PhoneNumber Parse(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var result = new PhoneNumberResult();
            result.Init(PhoneNumberParseThrowStyle.AllButOverflow);
            if (TryParsePhoneNumber(input, ref result))
            {
                return result.ParsedPhoneNumber;
            }
            else
            {
                throw result.GetPhoneNumberParseException();
            }
        }

        public static bool TryParse(string input, out PhoneNumber result)
        {
            var parseResult = new PhoneNumberResult();
            parseResult.Init(PhoneNumberParseThrowStyle.None);
            if (TryParsePhoneNumber(input, ref parseResult))
            {
                result = parseResult.ParsedPhoneNumber;
                return true;
            }
            else
            {
                result = PhoneNumber.Empty;
                return false;
            }
        }

        private static bool TryParsePhoneNumber(string input, ref PhoneNumberResult result)
        {
            if (input == null)
            {
                result.SetFailure("The parsed string cannot be null");
                return false;
            }

            // Remove whitespace, '-', '(', ')', extension numbers(after '*'), operator numbers(after ',').
            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                if ((c == '*') || (c == ','))
                {
                    break;
                }

                if (!char.IsWhiteSpace(c) && (c != '-') && (c != '(') && (c != ')'))
                {
                    sb.Append(c);
                }
            }
            string phoneNumberString = sb.ToString();

            if (phoneNumberString.Length == 0)
            {
                result.SetFailure("The parsed string cannot be an empty, whitespace or unmeaningful string");
                return false;
            }

            try
            {
                char[] c = phoneNumberString.ToCharArray();
                // Country code starts with "+" or "00".
                bool hasCountryCode = (c[0] == '+') || ((c[0] == '0') && (c[1] == '0'));

                // Skip the begining '+' or '0' characters
                int i = 0;
                while ((c[i] == '+') || (c[i] == '0'))
                {
                    i++;
                }

                long countryCode = _chinaCountryCode; // Default country is China(86)

                if (hasCountryCode)
                {
                    long code = 0;
                    for (int digit = 0; digit < 4; digit++)
                    {
                        var a = (c[i + digit]);
                        if (!Char.IsDigit(a))
                        {
                            result.SetFailure("Pphone number is designed to limit the digits only");
                            return false;
                        }

                        code = (code * 10) + (long)(a - '0');
                        if (s_internationalDialingCodesMap[code])
                        {
                            countryCode = (code << 48);
                            // Skip the country code
                            i += digit + 1;
                            break;
                        }
                    }
                }

                long phone = long.Parse(phoneNumberString.Substring(i));
                result.ParsedPhoneNumber = new PhoneNumber(phone | countryCode);
            }
            catch (Exception ex)
            {
                result.SetFailure("Failed to parse phone number string", ex);
                return false;
            }

            return true;
        }

        public override String ToString()
        {
            if (IsEmpty)
            {
                return "0";
            }

            PhoneNumberKind kind = Kind;
            int countryCode = CountryCode;

            if (kind == PhoneNumberKind.MobilePhone)
            {
                return "+" + countryCode + '-' + MobilePhone;
            }
            else if (kind == PhoneNumberKind.FixedPhone)
            {
                return "+" + countryCode + '-' + AreaCode + '-' + FixedPhone;
            }
            else
            {
                return "UnknownKind" + kind;
            }
        }

        public override int GetHashCode()
        {
            return _number.GetHashCode();
        }

        public int CompareTo(Guid other)
        {
            throw new NotImplementedException();
        }

        // Returns true if and only if the guid represented
        //  by o is the same as this instance.
        public override bool Equals(Object o)
        {
            PhoneNumber pn;
            // Check that o is a Guid first
            if (o == null || !(o is PhoneNumber))
            {
                return false;
            }
            else
            {
                pn = (PhoneNumber)o;
            }

            // Now compare each of the elements
            if (pn._number != _number)
            {
                return false;
            }
            return true;
        }

        public bool Equals(PhoneNumber pn)
        {
            // Now compare each of the elements
            if (pn._number != _number)
            {
                return false;
            }
            return true;
        }

        public int CompareTo(Object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is PhoneNumber))
            {
                throw new ArgumentException("value must be PhoneNumber type.");
            }
            PhoneNumber pn = (PhoneNumber)value;

            if (pn._number != this._number)
            {
                if (this._number < pn._number)
                {
                    return -1;
                }
                return 1;
            }

            return 0;
        }

        public int CompareTo(PhoneNumber value)
        {
            if (value._number != this._number)
            {
                if (this._number < value._number)
                {
                    return -1;
                }
                return 1;
            }

            return 0;
        }

        public static bool operator ==(PhoneNumber a, PhoneNumber b)
        {
            // Now compare each of the elements
            if (a._number != b._number)
            {
                return false;
            }

            return true;
        }

        public static bool operator !=(PhoneNumber a, PhoneNumber b)
        {
            return !(a == b);
        }
    }

}
