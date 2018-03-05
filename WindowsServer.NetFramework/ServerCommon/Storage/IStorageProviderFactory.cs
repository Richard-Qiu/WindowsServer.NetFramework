using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage
{
    public interface IStorageProviderFactory
    {
        IStorageCommand CreateCommand(IStorageConnection connection);
        IStorageConnection CreateConnection();
        IStorageConnection CreateConnection(string connectionString);
        StorageConnectionStringBuilder CreateConnectionStringBuilder();
    }
}
