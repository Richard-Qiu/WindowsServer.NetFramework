using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage
{
    public interface IStorageCommand : IDisposable
    {
        IStorageConnection Connection { get; }

        // File
        void CopyFile(string sourceFileName, string destFileName);
        void MoveFile(string sourceFileName, string destFileName);
        bool FileExists(string path);
        long GetFileLength(string path);
        byte[] ReadAllBytes(string path);
        string ReadAllText(string path);
        string ReadAllText(string path, Encoding encoding);
        Stream OpenFileRead(string path);
        Stream CreateFile(string path);
        void DeleteFile(string path);
        string GetRawAbsolutePath(string path);

        // Dirctory
        bool DirectoryExists(string path);
        bool CreateDirectoryIfNotExist(string path);
        string[] GetDirectoryFiles(string path, string searchPattern);

        // Storage-Local
        void DownloadFileFromStorage(string storageFilePath, string localFilePath);
        void UploadFileToStorage(string localFilePath, string storageFilePath);
    }
}
