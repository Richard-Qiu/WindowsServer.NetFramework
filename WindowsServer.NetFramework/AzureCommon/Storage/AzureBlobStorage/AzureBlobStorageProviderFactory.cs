using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Storage;

namespace WindowsServer.Azure.Storage.AzureBlobStorage
{
    public class AzureBlobStorageProviderFactory : IStorageProviderFactory
    {
        public IStorageCommand CreateCommand(IStorageConnection connection)
        {
            return new AzureBlobStorageCommand(connection as AzureBlobStorageConnection);
        }

        public IStorageConnection CreateConnection()
        {
            return new AzureBlobStorageConnection(this);
        }

        public IStorageConnection CreateConnection(string connectionString)
        {
            return new AzureBlobStorageConnection(connectionString, this);
        }

        public StorageConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new AzureBlobStorageConnectionStringBuilder();
        }
    }
}
