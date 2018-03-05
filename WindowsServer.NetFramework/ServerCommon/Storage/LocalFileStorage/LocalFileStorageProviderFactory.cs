using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage.LocalFileStorage
{
    public class LocalFileStorageProviderFactory : IStorageProviderFactory
    {
        public IStorageCommand CreateCommand(IStorageConnection connection)
        {
            return new LocalFileStorageCommand(connection as LocalFileStorageConnection);
        }

        public IStorageConnection CreateConnection()
        {
            return new LocalFileStorageConnection(this);
        }

        public IStorageConnection CreateConnection(string connectionString)
        {
            return new LocalFileStorageConnection(connectionString, this);
        }

        public StorageConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new LocalFileStorageConnectionStringBuilder();
        }
    }
}
