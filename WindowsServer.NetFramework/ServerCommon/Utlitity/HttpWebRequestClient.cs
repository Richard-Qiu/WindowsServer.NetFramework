using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Utlitity
{
    public class HttpWebRequestClient
    {
        private static void Init_Request(ref System.Net.HttpWebRequest request)
        {
            request.Accept = "text/json,*/*;q=0.5";
            request.Headers.Add("Accept-Charset", "utf-8;q=0.7,*;q=0.7");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, x-gzip, identity; q=0.9");
            request.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
            request.Timeout = 60000;
        }

        public static string Get(string url)
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(url);
                if (request != null)
                {
                    string retval = null;
                    Init_Request(ref request);
                    using (var Response = request.GetResponse())
                    {
                        using (var reader = new StreamReader(Response.GetResponseStream(), System.Text.Encoding.UTF8))
                        {
                            retval = reader.ReadToEnd();
                        }
                    }

                    return retval;
                }
            }
            catch (Exception ex)
            {
                //log.Error(ex);
            }

            return null;
        }
        public static string Post(string url, string data)
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(url);
                if (request != null)
                {
                    string retval = null;
                    Init_Request(ref request);
                    request.Method = "POST";
                    request.ServicePoint.Expect100Continue = false;
                    request.ContentType = "application/json; charset=utf-8";
                    var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(data);
                    request.ContentLength = bytes.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    using (var response = request.GetResponse())
                    {
                        using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                        {
                            retval = reader.ReadToEnd();
                        }
                    }

                    return retval;
                }
            }
            catch (Exception ex)
            {
                //log.Error(ex);
            }

            return null;
        }

        public static string PostHttps(string url, string data)
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(url);

                //HTTPSQ请求
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                if (request != null)
                {
                    string retval = null;
                    Init_Request(ref request);
                    request.Method = "POST";
                    request.ServicePoint.Expect100Continue = false;
                    request.ContentType = "application/json; charset=utf-8";
                    var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(data);
                    request.ContentLength = bytes.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    using (var response = request.GetResponse())
                    {
                        using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                        {
                            retval = reader.ReadToEnd();
                        }
                    }

                    return retval;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return null;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受   
        }
    }
}
