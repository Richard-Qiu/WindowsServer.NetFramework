using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage.LocalFileStorage
{
    public class LocalFileStorageConnection : IStorageConnection
    {
        private LocalFileStorageProviderFactory _factory;
        private string _connectionString;
        private string _rootPath;

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                var builder = new LocalFileStorageConnectionStringBuilder(value);
                _rootPath = builder.RootPath;
                _connectionString = builder.ToString();
            }
        }

        public int ConnectionTimeout
        {
            get
            {
                return 0; // No timeout
            }
        }

        public string Container
        {
            get { return "Default"; }
        }

        public StorageConnectionState State
        {
            get { return StorageConnectionState.Open; }
        }

        public IStorageProviderFactory StorageProviderFactory
        {
            get { return _factory; }
        }

        public string RootPath
        {
            get { return _rootPath; }
        }

        public LocalFileStorageConnection(string connectionString)
            : this(connectionString, null)
        {
        }

        public LocalFileStorageConnection(string connectionString, IStorageProviderFactory factory)
            : this(factory)
        {
            this.ConnectionString = new LocalFileStorageConnectionStringBuilder(connectionString).ToString(); ;
        }

        public LocalFileStorageConnection(IStorageProviderFactory factory)
        {
            _factory = (factory ?? StorageProviderFactories.GetFactoryByProviderName("LocalFile")) as LocalFileStorageProviderFactory;
        }

        public void Open()
        {
            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
        }

        public void Close()
        {
            // Do nothing for LocalFile storage
        }

        public IStorageCommand CreateCommand()
        {
            return new LocalFileStorageCommand(this);
        }

        public void Dispose()
        {
        }
    }
}
