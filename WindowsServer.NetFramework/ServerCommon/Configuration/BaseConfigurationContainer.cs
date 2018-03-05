using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsServer.Configuration
{
    public abstract class BaseConfigurationContainer
    {
        public abstract string GetSetting(string key);

        public abstract List<string> GetSettingKeys();
    }
}
