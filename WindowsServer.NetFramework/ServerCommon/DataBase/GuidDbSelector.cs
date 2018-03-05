using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.DataBase
{
    public class GuidDbSelector : SaasDbSelector<Guid>
    {
        private readonly string[] _connectionStrings = new string[256];
        private readonly string[] _distinctConnectionStrings = null;
        private readonly Guid[] _distinctShardingKeys = null;

        public string[] AllConnectionStrings
        {
            get { return _distinctConnectionStrings; }
        }

        public Guid[] AllShardingKeys
        {
            get { return _distinctShardingKeys; }
        }

        public GuidDbSelector(JsonObject configurationJsonObject)
        {
            foreach(var jsonPair in configurationJsonObject)
            {
                var range = jsonPair.Key.Split('-');
                var from = int.Parse(range[0], System.Globalization.NumberStyles.HexNumber);
                var to = 0;
                if (range.Length > 1)
                {
                    to = int.Parse(range[1], System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    to = from;
                }

                for (var i = from; i <= to; i++)
                {
                    if (string.IsNullOrEmpty(_connectionStrings[i]))
                    {
                        _connectionStrings[i] = (string)jsonPair.Value;
                    }
                    else
                    {
                        throw new Exception("GuidDbSelector has overlapped configuration on " + i + " partition.");
                    }
                }
            }

            // Check if all connection strings are covered
            for (int i = 0; i < _connectionStrings.Length; i++)
            {
                var connectionString = _connectionStrings[i];
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new Exception("GuidDbSelector's configuration does not cover all partitions. " + i + " partition is not covered.");
                }
            }

            var map = new Dictionary<string, byte>();
            for (int i = 0; i < _connectionStrings.Length; i++)
            {
                map[_connectionStrings[i]] = (byte)i;
            }
            _distinctConnectionStrings = map.Select(p => p.Key).ToArray();
            _distinctShardingKeys = map.Select(p => new Guid(new byte[] { 0, 0, 0, p.Value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })).ToArray();
        }

        public override string GetConnectionString(Guid guid)
        {
            var partition = guid.ToByteArray()[3];
            return _connectionStrings[partition];
        }

    }
}
