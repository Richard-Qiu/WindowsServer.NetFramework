using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindowsServer.Validation
{
    public static class MailValidator
    {
        private static readonly Regex _validatedByPeriod = new Regex("^\\.|\\.\\.|\\.$", RegexOptions.Compiled);
        private static readonly Regex _localPartValidatedByAsciiCharacter = new Regex("^\"(?:.)*\"$", RegexOptions.Compiled);
        private static readonly Regex _localPartValidatedByIncorrectUse = new Regex("(?:.)+[^\\\\]\"(?:.)+", RegexOptions.Compiled); //"(?:.)+[^\\\\]\"(?:.)+"
        private static readonly Regex _localPartValidatedByCorrectUse = new Regex(@"[ @\[\]\\"",]", RegexOptions.Compiled);//"[ @\\[\\]\\\\\",]"
        private static readonly Regex _domainPartValidatedByPeriod = new Regex("^\\[(.)+]$", RegexOptions.Compiled);
        private static readonly Regex _ipValidator = new Regex("\\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.Compiled);
        private static readonly Regex _validatedByHyphen = new Regex("^\\-|\\-$", RegexOptions.Compiled);
        private static readonly Regex _domainPartValidatedByCorrectUse = new Regex("(?:[0-9a-zA-Z][0-9a-zA-Z-]{0,61}[0-9a-zA-Z]|[a-zA-Z])(?:\\.|$)|(.)", RegexOptions.Compiled);
        /// <summary>
        /// http://tools.ietf.org/html/rfc3696
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool ValidateRfc3696Address(string address)
        {
            if (!address.Contains("@"))
            {
                return false;
            }
            if (address.EndsWith("@"))
            {
                return false;
            }
            var partIndex = address.LastIndexOf('@');
            var localPart = address.Substring(0, partIndex);
            var domainPart = address.Substring(partIndex + 1);
            if (localPart.Length < 1 || localPart.Length > 64)
            {
                return false;
            }
            if (domainPart.Length < 1 || domainPart.Length > 255)
            {
                return false;
            }
            var specialCharacters = new List<string> { "@", " ", "(", ")", ",", ":", ";", "<", ">", "[", "]", "\\", "\"" };

            var b = true;
            var quotationCount = localPart.Count(l => l == '"');
            var firstQuotationMarks = localPart.IndexOf('"');
            var lastQuotationMarks = localPart.LastIndexOf('"');

            specialCharacters.ForEach(x =>
                {
                    if (localPart.Contains(x))
                    {
                        if (quotationCount >= 2)
                        {
                            if ((firstQuotationMarks == 0 && lastQuotationMarks == localPart.Length - 1) || (firstQuotationMarks > 0 && localPart[firstQuotationMarks - 1] == '.' && localPart[lastQuotationMarks - 1] == '.'))
                            {
                                var signIndex = localPart.IndexOf(x);
                                if (signIndex < firstQuotationMarks || signIndex > lastQuotationMarks)
                                {
                                    b = false;
                                    return;
                                }
                            }
                            else
                            {
                                b = false;
                                return;
                            }

                        }
                        else
                        {
                            b = false;
                            return;
                        }
                    }
                });
            if (!b)
            {
                return false;
            }

            //    Period (".") may...appear, but may not be used to start or end the 
            //    local part, nor may two or more consecutive periods appear. 
            //        (http://tools.ietf.org/html/rfc3696#section-3) 
            if (_validatedByPeriod.IsMatch(localPart))
            {
                return false;
            }
            //    Any ASCII graphic (printing) character other than the 
            //    at-sign ("@"), backslash, double quote, comma, or square brackets may 
            //    appear without quoting.  If any of that list of excluded characters 
            //    are to appear, they must be quoted 
            //        (http://tools.ietf.org/html/rfc3696#section-3)
            if (_localPartValidatedByAsciiCharacter.IsMatch(localPart))
            {
                if (_localPartValidatedByIncorrectUse.IsMatch(localPart))
                {
                    return false;
                }
            }
            else
            {
                if (_localPartValidatedByCorrectUse.IsMatch(localPart))
                {
                    var st = _domainPartValidatedByCorrectUse.Replace(localPart, "");
                    if (_localPartValidatedByCorrectUse.IsMatch(st))
                    {
                        return false;
                    }
                }
            }

            //    The domain name can also be replaced by an IP address in square brackets 
            //        (http://tools.ietf.org/html/rfc3696#section-3) 
            //        (http://tools.ietf.org/html/rfc5321#section-4.1.3) 
            //        (http://tools.ietf.org/html/rfc4291#section-2.2) 
            if (_domainPartValidatedByPeriod.IsMatch(domainPart))
            {
                var ip = domainPart.Substring(1, domainPart.Length - 2);  //get ip
                var matchesIP = _ipValidator.Match(ip);

                var ipv6 = string.Empty;
                if (matchesIP.Length > 0)
                {
                    if (ip != matchesIP.Value)
                    {
                        if (ip.EndsWith(":"))
                        {
                            return false;
                        }
                        if (!ip.StartsWith("IPv6:"))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (!ip.StartsWith("IPv6:"))
                    {
                        return false;
                    }
                }

                //var ipv6Match = Regex.Match(ipv6, "^[0-9a-fA-F]{0,4}|\\:[0-9a-fA-F]{0,4}|(.)");
                //todo validate ipv6

            }
            else
            {
                //    Most common applications, including email and the Web, will generally not permit...escaped strings 
                //        (http://tools.ietf.org/html/rfc3696#section-2) 
                // 
                //    Characters outside the set of alphabetic characters, digits, and hyphen MUST NOT appear in domain name 
                //    labels for SMTP clients or servers 
                //        (http://tools.ietf.org/html/rfc5321#section-4.1.2) 
                // 
                //    RFC5321 precludes the use of a trailing dot in a domain name for SMTP purposes 
                //        (http://tools.ietf.org/html/rfc5321#section-4.1.2) 

                if (!domainPart.Contains("."))
                {
                    return false;
                }
                if (_validatedByPeriod.IsMatch(domainPart))
                {
                    return false;
                }
                if (!_domainPartValidatedByCorrectUse.IsMatch(domainPart))
                {
                    return false;
                }
                var parts = domainPart.Split('.');
                foreach (var p in parts)
                {
                    if (_validatedByHyphen.IsMatch(p))
                    {
                        return false;
                    }
                }
                if (Regex.IsMatch(parts[parts.Length - 1], "^[0-9]+$"))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
