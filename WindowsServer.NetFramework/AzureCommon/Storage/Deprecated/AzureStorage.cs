using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WindowsServer.Storage;

namespace WindowsServer.Azure.Storage
{
    public class AzureStorage : BaseStorage
    {
        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;

        public AzureStorage(string connectionString, string containerName)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(containerName);
            if (_blobContainer.CreateIfNotExists())
            {
                var containerPermissions = new BlobContainerPermissions();
                containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
                _blobContainer.SetPermissions(containerPermissions);
            }
        }

        public override string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public override string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public override string GetFileExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public override string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path).Replace('\\', '/');
        }

        public override string PathCombine(string path1, string path2)
        {
            return Path.Combine(path1, path2).Replace('\\', '/');
        }

        public override void CopyFile(string sourceFileName, string destFileName)
        {
            var destFileReference = _blobContainer.GetBlockBlobReference(destFileName);
            var sourceFileReference = _blobContainer.GetBlockBlobReference(sourceFileName);
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

        public override void MoveFile(string sourceFileName, string destFileName)
        {
            var destFileReference = _blobContainer.GetBlockBlobReference(destFileName);
            var sourceFileReference = _blobContainer.GetBlockBlobReference(sourceFileName);
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

        public override bool FileExists(string path)
        {
            var file = _blobContainer.GetBlockBlobReference(path);
            return file.Exists();
        }

        public override long GetFileLength(string path)
        {
            var file = _blobContainer.GetBlockBlobReference(path);
            file.FetchAttributes();
            return file.Properties.Length;
        }

        public override byte[] ReadFileAllBytes(string path)
        {
            var file = _blobContainer.GetBlockBlobReference(path);
            file.FetchAttributes();
            var bytes = new byte[file.Properties.Length];
            file.DownloadToByteArray(bytes, 0);
            return bytes;
        }

        public override string ReadAllText(string path, Encoding encoding)
        {
            var file = _blobContainer.GetBlockBlobReference(path);
            var text = file.DownloadText(encoding);
            if ((text.Length > 0) && (text[0] == 0xFEFF))
            {
                return text.Substring(1);
            }
            return text;
        }

        public override System.IO.Stream OpenFileRead(string path)
        {
            var file = _blobContainer.GetBlockBlobReference(path);
            return file.OpenRead();
        }

        public override System.IO.Stream CreateFile(string path)
        {
            var file = _blobContainer.GetBlockBlobReference(path);
            file.DeleteIfExists();
            return file.OpenWrite();
        }

        public override void DeleteFile(string path)
        {
            var file = _blobContainer.GetBlockBlobReference(path);
            file.DeleteIfExists();
        }



        public override bool DirectoryExists(string path)
        {
            // Azure blob storage does not have traditional concept of directory.
            // Always return true here.
            return true;
        }

        public override bool CreateDirectoryIfNotExist(string path)
        {
            // Azure blob storage does not have traditional concept of directory.
            // Always return false here, meaning that the directory exists already.
            return false;
        }

        public override string[] GetDirectoryFiles(string path, string searchPattern)
        {
            var files = new List<string>();

            searchPattern = searchPattern.Replace(".", "\\.");
            searchPattern = searchPattern.Replace('?', '.');
            searchPattern = searchPattern.Replace("*", ".*");
            var regex = new Regex(
                "^" + searchPattern + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

            var dir = _blobContainer.GetDirectoryReference(path);
            BlobResultSegment result = null;
            do
            {
                result = dir.ListBlobsSegmented(
                    false,
                    BlobListingDetails.None,
                    null,
                    (result == null) ? null : result.ContinuationToken,
                    null,
                    null);
                foreach (var item in result.Results)
                {
                    var blob = item as CloudBlockBlob;
                    if (blob == null)
                    {
                        continue;
                    }

                    var name = GetFileName(blob.Name);
                    if (regex.IsMatch(name))
                    {
                        files.Add(blob.Name);
                    }
                }
            } while (result.ContinuationToken != null);

            return files.ToArray();
        }

        public override void DownloadFileFromStorage(string storageFilePath, string localFilePath)
        {
            var file = _blobContainer.GetBlockBlobReference(storageFilePath);
            file.DownloadToFile(localFilePath, FileMode.Create);
        }

        public override void UploadFileToStorage(string localFilePath, string storageFilePath)
        {
            var file = _blobContainer.GetBlockBlobReference(storageFilePath);
            file.UploadFromFile(localFilePath, FileMode.Open);
        }
    }
}
