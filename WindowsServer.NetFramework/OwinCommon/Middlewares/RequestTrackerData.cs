using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Middlewares
{
    internal class RequestTrackerData
    {
        public Guid Id { get; set; }
        public IDictionary<string, object> Environment { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
        public long RequestBodyLength { get; set; }
        public byte[] RequestBody { get; set; }
        public int ResponseHttpCode { get; set; }
        public string Exception { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; }
        public long ResponseBodyLength { get; set; }
        public byte[] ResponseBody { get; set; }
        public DateTime ProcessingTime { get; set; }
        public DateTime ProcessedTime { get; set; }

        private string _accessToken = null;
        public string AccessToken
        {
            get
            {
                if (_accessToken == null)
                {
                    if (!RequestHeaders.TryGetValue("AccessToken", out _accessToken))
                    {
                        _accessToken = string.Empty;
                    }
                }
                return _accessToken;
            }
        }
    }
}
