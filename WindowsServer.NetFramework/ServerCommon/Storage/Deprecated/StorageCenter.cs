using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage
{
    public static class StorageCenter
    {
        private static object _syncObject = new object();

        private static bool _hasSearchedAzureAssembly = false;
        private static Assembly _azureAssembly = null;
        private static string _azureStorageClassName = "WindowsServer.Azure.Storage.AzureStorage";

        public static BaseStorage CreateSystemStorage()
        {
            return new SystemStorage();
        }

        public static BaseStorage CreateAzureStorage(string connectionString, string containerName)
        {
            if (!_hasSearchedAzureAssembly)
            {
                lock(_syncObject)
                {
                    if (!_hasSearchedAzureAssembly)
                    {
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            var t = assembly.GetType(_azureStorageClassName);
                            if (t != null)
                            {
                                _azureAssembly = assembly;
                                break;
                            }
                        }
                        _hasSearchedAzureAssembly = true;
                    }
                }
            }

            var storage = (BaseStorage)_azureAssembly.CreateInstance(
                _azureStorageClassName,
                false,
                BindingFlags.CreateInstance,
                null,
                new object[] { connectionString, containerName },
                null,
                null);
            if (storage == null)
            {
                throw new Exception("Failed to create AzureStorage object");
            }
            return storage;
        }

    }
}
