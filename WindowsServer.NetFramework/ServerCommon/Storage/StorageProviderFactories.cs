using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Storage.LayeredStorage;
using WindowsServer.Storage.LocalFileStorage;

namespace WindowsServer.Storage
{
    public static class StorageProviderFactories
    {
        private static object s_syncObject = new object();

        private static bool s_hasSearchedAzureAssembly = false;
        private static Assembly s_azureBlobStorageProviderFactoryAssembly = null;


        private static Dictionary<string, IStorageProviderFactory> s_cached = new Dictionary<string, IStorageProviderFactory>();

        private static IStorageProviderFactory GetCachedFactory(string key, Func<IStorageProviderFactory> builder)
        {
            IStorageProviderFactory factory;
            if (!s_cached.TryGetValue(key, out factory))
            {
                lock (s_cached)
                {
                    if (!s_cached.TryGetValue(key, out factory))
                    {
                        factory = builder();
                        s_cached.Add(key, factory);
                    }
                }
            }
            return factory;
        }

        public static IStorageProviderFactory GetFactoryByConnectionString(string connectionString)
        {
            var json = JsonObject.Parse(connectionString) as JsonObject;
            return GetFactoryByProviderName(json.GetStringValue("Type"));
        }

        public static IStorageProviderFactory GetFactoryByProviderName(string providerInvariantName)
        {
            switch (providerInvariantName)
            {
                case "LocalFile":
                case "WindowsServer.Storage.LocalFileStorage.LocalFileStorageProviderFactory":
                    return GetCachedFactory("LocalFile", () => new LocalFileStorageProviderFactory());

                case "Layered":
                case "WindowsServer.Storage.LayeredStorage.LayeredStorageProviderFactory":
                    return GetCachedFactory("Layered", () => new LayeredStorageProviderFactory());

                case "AzureBlob":
                case "WindowsServer.Azure.Storage.AzureBlobStorage.AzureBlobStorageProviderFactory":
                    if (!s_hasSearchedAzureAssembly)
                    {
                        lock (s_syncObject)
                        {
                            if (!s_hasSearchedAzureAssembly)
                            {
                                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                                {
                                    var t = assembly.GetType("WindowsServer.Azure.Storage.AzureBlobStorage.AzureBlobStorageProviderFactory");
                                    if (t != null)
                                    {
                                        s_hasSearchedAzureAssembly = true;
                                        s_azureBlobStorageProviderFactoryAssembly = assembly;
                                    }
                                }
                            }
                        }
                    }

                    var factory = (IStorageProviderFactory)s_azureBlobStorageProviderFactoryAssembly.CreateInstance(
                        "WindowsServer.Azure.Storage.AzureBlobStorage.AzureBlobStorageProviderFactory",
                        true,
                        BindingFlags.CreateInstance,
                        null,
                        null,
                        null,
                        null);
                    if (factory == null)
                    {
                        throw new Exception("Failed to create AzureBlobStorage object");
                    }
                    return GetCachedFactory("AzureBlob", () => factory);
                default:
                    throw new Exception("Unknown provider: " + providerInvariantName);
            }
        }

    }
}
