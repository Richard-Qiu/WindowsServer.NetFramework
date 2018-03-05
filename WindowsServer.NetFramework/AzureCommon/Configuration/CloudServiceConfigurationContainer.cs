using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsServer.Configuration;

namespace WindowsServer.Azure.Configuration
{
    public class CloudServiceConfigurationContainer : BaseConfigurationContainer
    {
        public override string GetSetting(string key)
        {
            return CloudConfigurationManager.GetSetting(key);
        }

        public override List<string> GetSettingKeys()
        {
            return new List<string>();
        }
    }
}
