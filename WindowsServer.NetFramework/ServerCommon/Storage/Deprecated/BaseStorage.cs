using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage
{
    public abstract class BaseStorage
    {
        // Path
        public abstract string GetFileName(string path);
        public abstract string GetFileNameWithoutExtension(string path);
        public abstract string GetFileExtension(string path);
        public abstract string GetDirectoryName(string path);
        public abstract string PathCombine(string path1, string path2);

        // File
        public abstract void CopyFile(string sourceFileName, string destFileName);
        public abstract void MoveFile(string sourceFileName, string destFileName);
        public abstract bool FileExists(string path);
        public abstract long GetFileLength(string path);
        public abstract byte[] ReadFileAllBytes(string path);
        public abstract string ReadAllText(string path, Encoding encoding);
        public abstract Stream OpenFileRead(string path);
        public abstract Stream CreateFile(string path);
        public abstract void DeleteFile(string path);

        // Dirctory
        public abstract bool DirectoryExists(string path);
        public abstract bool CreateDirectoryIfNotExist(string path);
        public abstract string[] GetDirectoryFiles(string path, string searchPattern);

        // Storage-Local
        public abstract void DownloadFileFromStorage(string storageFilePath, string localFilePath);
        public abstract void UploadFileToStorage(string localFilePath, string storageFilePath);
    }
}
