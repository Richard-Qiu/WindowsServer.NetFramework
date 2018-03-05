using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage.LayeredStorage
{
    public class LayeredStorageProviderFactory : IStorageProviderFactory
    {
        public IStorageCommand CreateCommand(IStorageConnection connection)
        {
            return new LayeredStorageCommand(connection as LayeredStorageConnection);
        }

        public IStorageConnection CreateConnection()
        {
            return new LayeredStorageConnection(this);
        }

        public IStorageConnection CreateConnection(string connectionString)
        {
            return new LayeredStorageConnection(connectionString, this);
        }

        public StorageConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new LayeredStorageConnectionStringBuilder();
        }
    }
}
