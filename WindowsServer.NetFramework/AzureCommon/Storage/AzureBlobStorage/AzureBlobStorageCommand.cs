using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WindowsServer.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace WindowsServer.Azure.Storage.AzureBlobStorage
{
    public class AzureBlobStorageCommand : IStorageCommand
    {
        private AzureBlobStorageConnection _connection;
        private CloudBlobContainer _container;

        public AzureBlobStorageCommand(AzureBlobStorageConnection connection)
        {
            _connection = connection;
            _container = connection.BlobContainer;
        }

        public IStorageConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        public void CopyFile(string sourceFileName, string destFileName)
        {
            var destFileReference = _container.GetBlockBlobReference(destFileName);
            var sourceFileReference = _container.GetBlockBlobReference(sourceFileName);

            destFileReference.StartCopyFromBlob(sourceFileReference);
            while (true)
            {
                destFileReference.FetchAttributes();
                if (destFileReference.CopyState.Status != CopyStatus.Pending)
                {
                    break;
                }
                System.Threading.Thread.Sleep(200);
            }

            if (destFileReference.CopyState.Status != CopyStatus.Success)
            {
                throw new Exception("Failed to copy file from " + sourceFileName + " to " + destFileName + ". Result:" + destFileReference.CopyState.Status);
            }
        }

        public void MoveFile(string sourceFileName, string destFileName)
        {
            var destFileReference = _container.GetBlockBlobReference(destFileName);
            var sourceFileReference = _container.GetBlockBlobReference(sourceFileName);
            destFileReference.StartCopyFromBlob(sourceFileReference);
            while (true)
            {
                destFileReference.FetchAttributes();
                if (destFileReference.CopyState.Status != CopyStatus.Pending)
                {
                    break;
                }
                System.Threading.Thread.Sleep(200);
            }

            if (destFileReference.CopyState.Status != CopyStatus.Success)
            {
                destFileReference.DeleteIfExists();
                throw new Exception("Failed to move file from " + sourceFileName + " to " + destFileName + ". Result:" + destFileReference.CopyState.Status);
            }
            sourceFileReference.Delete();
        }

        public bool FileExists(string path)
        {
            var file = _container.GetBlockBlobReference(path);
            return file.Exists();
        }

        public long GetFileLength(string path)
        {
            var file = _container.GetBlockBlobReference(path);
            file.FetchAttributes();
            return file.Properties.Length;
        }

        public byte[] ReadAllBytes(string path)
        {
            var file = _container.GetBlockBlobReference(path);
            file.FetchAttributes();
            var bytes = new byte[file.Properties.Length];
            file.DownloadToByteArray(bytes, 0);
            return bytes;
        }

        public string ReadAllText(string path)
        {
            return ReadAllText(path, Encoding.UTF8);
        }

        public string ReadAllText(string path, Encoding encoding)
        {
            var file = _container.GetBlockBlobReference(path);
            return file.DownloadText(encoding);
        }

        public Stream OpenFileRead(string path)
        {
            var file = _container.GetBlockBlobReference(path);
            return file.OpenRead();
        }

        public Stream CreateFile(string path)
        {
            var file = _container.GetBlockBlobReference(path);
            file.DeleteIfExists();
            return file.OpenWrite();
        }

        public void DeleteFile(string path)
        {
            var file = _container.GetBlockBlobReference(path);
            file.DeleteIfExists();
        }

        public string GetRawAbsolutePath(string path)
        {
            var file = _container.GetBlockBlobReference(path);
            return file.Uri.ToString();
        }

        public bool DirectoryExists(string path)
        {
            var directory = _container.GetDirectoryReference(path);
            return (directory.ListBlobs().Count() > 0); // In blob storage, Directories don't exist as an item by themselves
        }

        public bool CreateDirectoryIfNotExist(string path)
        {
            if (DirectoryExists(path))
            {
                return false;
            }
            return true;
            //throw new NotSupportedException("In blob storage, Directories don't exist as an item by themselves.");
        }

        public string[] GetDirectoryFiles(string path, string searchPattern)
        {
            var directory = _container.GetDirectoryReference(path);

            var blobUrls = new List<string>();
            foreach (var blob in directory.ListBlobs().OfType<CloudBlockBlob>())
	        {
                if (blob.Name.Contains(searchPattern))
                {
                    blobUrls.Add(blob.Uri.ToString());
                }
	        }
            return blobUrls.ToArray();
        }

        public void DownloadFileFromStorage(string storageFilePath, string localFilePath)
        {
            var file = _container.GetBlockBlobReference(storageFilePath);
            file.DownloadToFile(localFilePath, FileMode.Create);
        }

        public void UploadFileToStorage(string localFilePath, string storageFilePath)
        {
            var file = _container.GetBlockBlobReference(storageFilePath);
            using (var fileStream = File.OpenRead(localFilePath))
            {
                file.UploadFromStream(fileStream);
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
