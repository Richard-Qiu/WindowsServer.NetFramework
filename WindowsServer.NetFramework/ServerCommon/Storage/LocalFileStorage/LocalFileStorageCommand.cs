using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage.LocalFileStorage
{
    public class LocalFileStorageCommand : IStorageCommand
    {
        private LocalFileStorageConnection _connection;
        private string _rootPath;

        public LocalFileStorageCommand(LocalFileStorageConnection connection)
        {
            _connection = connection;
            _rootPath = _connection.RootPath;
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
            File.Copy(Path.Combine(_rootPath, sourceFileName), Path.Combine(_rootPath, destFileName));
        }

        public void MoveFile(string sourceFileName, string destFileName)
        {
            File.Move(Path.Combine(_rootPath, sourceFileName), Path.Combine(_rootPath, destFileName));
        }

        public bool FileExists(string path)
        {
            return File.Exists(Path.Combine(_rootPath, path));
        }

        public long GetFileLength(string path)
        {
            return new FileInfo(Path.Combine(_rootPath, path)).Length;
        }

        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(Path.Combine(_rootPath, path));
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(Path.Combine(_rootPath, path));
        }

        public string ReadAllText(string path, Encoding encoding)
        {
            return File.ReadAllText(Path.Combine(_rootPath, path), encoding);
        }

        public Stream OpenFileRead(string path)
        {
            return File.OpenRead(Path.Combine(_rootPath, path));
        }

        public Stream CreateFile(string path)
        {
            path = Path.Combine(_rootPath, path);
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return File.Create(path);
        }

        public void DeleteFile(string path)
        {
            File.Delete(Path.Combine(_rootPath, path));
        }

        public string GetRawAbsolutePath(string path)
        {
            return Path.Combine(_rootPath, path);
        }



        public bool DirectoryExists(string path)
        {
            return Directory.Exists(Path.Combine(_rootPath, path));
        }

        public bool CreateDirectoryIfNotExist(string path)
        {
            path = Path.Combine(_rootPath, path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }

        public string[] GetDirectoryFiles(string path, string searchPattern)
        {
            var fullPath = Path.Combine(_rootPath, path);
            if (Directory.Exists(fullPath))
            {
                return Directory.GetFiles(fullPath, searchPattern);
            }
            return null;
        }



        public void DownloadFileFromStorage(string storageFilePath, string localFilePath)
        {
            File.Copy(Path.Combine(_rootPath, storageFilePath), localFilePath);
        }

        public void UploadFileToStorage(string localFilePath, string storageFilePath)
        {
            File.Copy(localFilePath, Path.Combine(_rootPath, storageFilePath));
        }

        public void Dispose()
        {
            // No need to dispose
        }
    }
}
