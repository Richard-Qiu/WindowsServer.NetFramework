using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using WindowsServer.Log;

namespace WindowsServer.Configuration
{
    public class ConfigurationCenter
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static ContainerWrapper Local { get; private set; }
        public static ContainerWrapper Global { get; private set; }

        public class ContainerWrapper
        {
            private BaseConfigurationContainer _container;
            private BaseConfigurationContainer _fallbackContainer;
            private BaseConfigurationContainer _fallback2Container;

            internal ContainerWrapper(BaseConfigurationContainer container, BaseConfigurationContainer fallbackContainer = null, BaseConfigurationContainer fallback2Container = null)
            {
                _container = container;
                _fallbackContainer = fallbackContainer;
                _fallback2Container = fallback2Container;
            }

            public override string ToString()
            {
                return string.Format(
                    "Container:{0} Fallback:{1} Fallback2:{2}",
                    _container.GetType().ToString(),
                    _fallbackContainer == null ? "null" : _fallbackContainer.GetType().ToString(),
                    _fallback2Container == null ? "null" : _fallback2Container.GetType().ToString()
                );
            }

            public string this[string key]
            {
                get
                {
                    var v = _container.GetSetting(key);
                    if ((v == null) && (_fallbackContainer != null))
                    {
                        v = _fallbackContainer.GetSetting(key);
                        if ((v == null) && (_fallback2Container != null))
                        {
                            v = _fallback2Container.GetSetting(key);
                        }
                    }
                    return v;
                }
            }

            public List<string> DiscoverAllKeys()
            {
                var keys = new List<string>();
                keys.AddRange(_container.GetSettingKeys());
                if (_fallbackContainer != null)
                {
                    keys.AddRange(_fallbackContainer.GetSettingKeys());
                }
                if (_fallback2Container != null)
                {
                    keys.AddRange(_fallback2Container.GetSettingKeys());
                }
                return keys;
            }
        }

        public static void Initialize(bool globalFallbackToLocal = true, string rootPath = "")
        {
            Local = new ContainerWrapper(new FileConfigurationContainer(), new FederationConfigurationContainer(rootPath));

            bool useAzureConfiguration = bool.Parse(ConfigurationManager.AppSettings["UseAzureConfiguration"] ?? "false");
            if (useAzureConfiguration)
            {
                AppDomain.CurrentDomain.Load("WindowsServer.Azure.Common");

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var cloudServiceType = assembly.GetType("WindowsServer.Azure.Configuration.CloudServiceConfigurationContainer", false, false);
                    if (cloudServiceType != null)
                    {
                        var container = assembly.CreateInstance(cloudServiceType.FullName) as BaseConfigurationContainer;
                        if (container == null)
                        {
                            var error = "Cannot convert CloudServiceConfigurationContainer to BaseConfigurationContainer";
                            _logger.Error(error);
                            throw new Exception(error);
                        }
                        else
                        {
                            if (globalFallbackToLocal)
                            {
                                Global = new ContainerWrapper(container, new FileConfigurationContainer(), new FederationConfigurationContainer(rootPath));
                            }
                            else
                            {
                                Global = new ContainerWrapper(container, new FederationConfigurationContainer(rootPath));
                            }
                        }
                        break;
                    }
                }
            }

            if (Global == null)
            {
                Global = new ContainerWrapper(new FileConfigurationContainer(), new FederationConfigurationContainer(rootPath));
            }

            _logger.Info("ConfigurationCenter.Global is " + Global.ToString());
        }
    }
}
