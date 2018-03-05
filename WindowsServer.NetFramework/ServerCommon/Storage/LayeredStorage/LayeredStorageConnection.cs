using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage.LayeredStorage
{
    public class LayeredStorageConnection : IStorageConnection
    {
        private LayeredStorageProviderFactory _factory;
        private string _connectionString;
        private IStorageConnection[] _layers;

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                var builder = new LayeredStorageConnectionStringBuilder(value);
                _layers = builder.Layers;
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

        public IStorageConnection[] Layers
        {
            get { return _layers; }
        }

        public LayeredStorageConnection(string connectionString)
            : this(connectionString, null)
        {
        }

        public LayeredStorageConnection(string connectionString, IStorageProviderFactory factory)
            : this(factory)
        {
            this.ConnectionString = new LayeredStorageConnectionStringBuilder(connectionString).ToString(); ;
        }

        public LayeredStorageConnection(IStorageProviderFactory factory)
        {
            _factory = (factory ?? StorageProviderFactories.GetFactoryByProviderName("Layered")) as LayeredStorageProviderFactory;
        }

        public void Open()
        {
            foreach(var layer in _layers.Reverse())
            {
                layer.Open();
            }
        }

        public void Close()
        {
            foreach (var layer in _layers)
            {
                layer.Close();
            }
        }

        public IStorageCommand CreateCommand()
        {
            return new LayeredStorageCommand(this);
        }

        ~LayeredStorageConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var layer in _layers)
                {
                    layer.Dispose();
                }
                _layers = null;
            }
        }

    }
}
