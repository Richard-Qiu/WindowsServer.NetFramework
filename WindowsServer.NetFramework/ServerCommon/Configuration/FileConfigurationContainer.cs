using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace WindowsServer.Configuration
{
    internal class FileConfigurationContainer : BaseConfigurationContainer
    {
        public override string GetSetting(string key)
        {
            var v = ConfigurationManager.AppSettings[key];
            if (v == null)
            {
                var conn = ConfigurationManager.ConnectionStrings[key];
                if (conn != null)
                {
                    v = conn.ConnectionString;
                }
            }
            return v;
        }

        public override List<string> GetSettingKeys()
        {
            var keys = new List<string>();
            keys.AddRange(ConfigurationManager.AppSettings.AllKeys);
            for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
            {
                keys.Add(ConfigurationManager.ConnectionStrings[i].Name);
            }
            return keys;
        }

    }
}
