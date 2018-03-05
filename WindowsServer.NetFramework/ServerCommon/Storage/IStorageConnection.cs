using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage
{
    public interface IStorageConnection : IDisposable
    {
        string ConnectionString { get; set; }
        int ConnectionTimeout { get; }
        string Container { get; }
        StorageConnectionState State { get; }
        IStorageProviderFactory StorageProviderFactory { get; }
        void Open();
        void Close();
        IStorageCommand CreateCommand();
    }
}
