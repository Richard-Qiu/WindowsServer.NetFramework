using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Log;

namespace WindowsServer.Configuration
{
    internal class FederationConfigurationContainer : BaseConfigurationContainer
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        public FederationConfigurationContainer(string rootPath)
        {
            var appSetting = ConfigurationManager.AppSettings["FederationConfiguration"];
            if (string.IsNullOrEmpty(appSetting))
            {
                s_logger.Info("FederationConfiguration is not set.");
                return;
            }
            var parts = appSetting.Split(new char[] { '@' }, 2);
            if (parts.Length != 2)
            {
                var message = "FederationConfiguration is not in format \"[name]@[federationConfigurationFilePath]\". The value is " + appSetting;
                s_logger.Error(message);
                throw new Exception(message);
            }

            var configurationFilePath = Path.Combine(rootPath, parts[1]);
            LoadSettings(parts[0], configurationFilePath);
        }

        private void LoadSettings(string name, string configurationFilePath)
        {
            var jsonString = File.ReadAllText(configurationFilePath);
            var json = JsonValue.Parse(jsonString) as JsonObject;

            foreach (var group in json)
            {
                var inScope = false;
                var key = group.Key;
                if (key.Equals("all"))
                {
                    inScope = true;
                }
                else
                {
                    var memberNames = key.Split(';').Select(n => n.Trim().ToLowerInvariant()).ToList();
                    inScope = memberNames.Contains(name.ToLowerInvariant());
                }

                if (!inScope)
                {
                    continue;
                }

                foreach(var settingJson in group.Value as JsonObject)
                {
                    var settingKey = settingJson.Key;
                    var settingValue = settingJson.Value.JsonType == JsonType.String ? (string)settingJson.Value : settingJson.Value.ToString();

                    if (_settings.ContainsKey(settingKey))
                    {
                        s_logger.Warn($"The setting key '{settingKey}' will be overwritten. The old value is '{_settings[settingKey]}'. The new value is '{settingValue}'.");
                    }
                    _settings[settingKey] = settingValue;
                }
            }
        }

        public override string GetSetting(string key)
        {
            string value;
            if (_settings.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        public override List<string> GetSettingKeys()
        {
            return _settings.Keys.ToList();
        }

    }
}
