using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Text;

namespace WindowsServer.Web
{
    public static class MailUtility
    {
        public static SmtpClient CreateSmtpClientFromConfiguration(string configurationString)
        {
            var pairs = configurationString.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
            var configurations = new Dictionary<string, string>();
            foreach(var pair in pairs)
            {
                var p = pair.Split('=');
                configurations.Add(p[0].ToLowerInvariant(), p[1]);
            }

            var client = new SmtpClient();

            client.Host = configurations["host"];

            if (configurations.ContainsKey("port"))
            {
                client.Port = int.Parse(configurations["port"]);
            }

            if (configurations.ContainsKey("enablessl"))
            {
                client.EnableSsl = bool.Parse(configurations["enablessl"]);
            }

            if (configurations.ContainsKey("usedefaultcredentials"))
            {
                client.UseDefaultCredentials = bool.Parse(configurations["usedefaultcredentials"]);
            }

            if (configurations.ContainsKey("username"))
            {
                var userName = configurations["username"];
                var password = configurations["password"];
                if (configurations.ContainsKey("domain"))
                {
                    client.Credentials = new NetworkCredential(userName, password, configurations["domain"]);
                }
                else
                {
                    client.Credentials = new NetworkCredential(userName, password);
                }
            }

            return client;
        }

        public static string GetDomainName(string mailAddress)
        {
            var domainName = mailAddress.Substring(mailAddress.IndexOf('@') + 1);
            return domainName;
        }

        public static string GetAccountNameWithoutDomain(string mailAddress)
        {
            var accountName = mailAddress.Substring(0, mailAddress.IndexOf('@'));
            return accountName;
        }

        /// <summary>
        /// Compare the specified mail address again correctMailDomainNames, and try to correct the given mailAddress.
        /// </summary>
        /// <param name="correctMailDomainNames">The collection of correct mail domain names.</param>
        /// <param name="mailAddress">The mail address which will be corrected. It should be a valid mail address format. Otherwise, an exception will be thrown</param>
        /// <returns>The corrected mail address. If empty string, it indicates the given mailAddress is already correct.</returns>
        public static string CorrectMailAddress(IEnumerable<string> correctMailDomainNames, string mailAddress)
        {
            var correctedMailAddress = StringUtility.RemoveWhiteSpaces(mailAddress);
            correctedMailAddress = StringUtility.ToDBC(correctedMailAddress);

            // Simply parsing the given mailAddress
            var atIndex = correctedMailAddress.IndexOf('@');
            if ((atIndex <= 0) || (atIndex == (correctedMailAddress.Length - 1)))
            {
                throw new FormatException("The given mailAddress format is invalid. " + mailAddress);
            }
            var account = correctedMailAddress.Substring(0, atIndex);
            var domainName = correctedMailAddress.Substring(atIndex + 1);
            //var lastDotIndex = domainName.LastIndexOf('.');
            //if ((lastDotIndex <= 0) || (lastDotIndex == (domainName.Length - 1)))
            //{
            //    throw new FormatException("The given mailAddress format is invalid. " + mailAddress);
            //}
            //var domainNameFirst = correctedMailAddress.Substring(0, lastDotIndex);
            //var domainNameSecond = correctedMailAddress.Substring(lastDotIndex + 1);
            var minDistance = 99;
            var minDistanceDomainName = string.Empty;
            var moreThanOne = false;
            foreach (var correctMailDomainName in correctMailDomainNames)
            {
                var distance = StringUtility.CalculateLevenshteinDistance(domainName, correctMailDomainName);
                if (distance <= 0)
                {
                    // Exactly match, return
                    return ComputeCorrectMailAddressResult(mailAddress, account, correctMailDomainName);
                }

                if (minDistance == distance)
                {
                    // Find one more candidate
                    moreThanOne = true;
                }
                else if (minDistance > distance)
                {
                    minDistance = distance;
                    moreThanOne = false;
                    minDistanceDomainName = correctMailDomainName;
                }
            }

            if (moreThanOne)
            {
                // If there are more than one candidates which have same distance, do not correct.
                return ComputeCorrectMailAddressResult(mailAddress, account, domainName);
            }
            else
            {
                if (minDistance < 2)
                {
                    // Only correct 1 difference mailAddress
                    return ComputeCorrectMailAddressResult(mailAddress, account, minDistanceDomainName);
                }
                else
                {
                    // Do not correct
                    return ComputeCorrectMailAddressResult(mailAddress, account, domainName);
                }
            }
        }

        private static string ComputeCorrectMailAddressResult(string originalMailAddress, string correctedAccount, string correctedDomainName)
        {
            return correctedAccount + "@" + correctedDomainName;
        }

    }
}
