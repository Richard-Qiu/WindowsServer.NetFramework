using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Storage;

namespace WindowsServer.Azure.Storage.AzureBlobStorage
{
    public class AzureBlobStorageConnection : IStorageConnection
    {
        private string _connectionString;
        private StorageConnectionState _state;
        private AzureBlobStorageProviderFactory _factory;

        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                var builder = new AzureBlobStorageConnectionStringBuilder(value);
                _connectionString = builder.ToString();

                var blobStorageConnectionString = string.Format("DefaultEndpointsProtocol={0};BlobEndpoint={1};AccountName={2};AccountKey={3};",
                    builder.EndpointsProtocol,
                    builder.Endpoint,
                    builder.AccountName,
                    builder.AccountKey);

                _storageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
                _blobClient = _storageAccount.CreateCloudBlobClient();
                _blobContainer = _blobClient.GetContainerReference(builder.Container);
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
            get { return _blobContainer.Name; }
        }

        public StorageConnectionState State
        {
            get { return _state; }
        }

        public IStorageProviderFactory StorageProviderFactory
        {
            get { return _factory; }
        }

        public CloudStorageAccount StorageAccount
        {
            get { return _storageAccount; }
        }

        public CloudBlobClient BlobClient
        {
            get { return _blobClient; }
        }

        public CloudBlobContainer BlobContainer
        {
            get { return _blobContainer; }
        }

        public AzureBlobStorageConnection(string connectionString)
            :this(connectionString, null)
        { 
        }

        public AzureBlobStorageConnection(string connectionString, AzureBlobStorageProviderFactory factory)
            : this(factory)
        {
            this.ConnectionString = connectionString;
        }

        public AzureBlobStorageConnection(AzureBlobStorageProviderFactory factory)
        {
            _factory = (factory ?? StorageProviderFactories.GetFactoryByProviderName("AzureBlob")) as AzureBlobStorageProviderFactory;
        }

        public void Open()
        {
            if (_blobContainer.CreateIfNotExists())
            {
                var blobContainerPermisson = new BlobContainerPermissions();
                blobContainerPermisson.PublicAccess = BlobContainerPublicAccessType.Container;
                _blobContainer.SetPermissions(blobContainerPermisson);
            }
            _state = StorageConnectionState.Open;
        }

        public void Close()
        {
            _state = StorageConnectionState.Closed;
            Dispose();
        }

        public IStorageCommand CreateCommand()
        {
            return new AzureBlobStorageCommand(this);
        }

        public void Dispose()
        {

        }
    }
}
