using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage
{
    internal class SystemStorage : BaseStorage
    {
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
            return Path.GetDirectoryName(path);
        }

        public override string PathCombine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }



        public override void CopyFile(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public override void MoveFile(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        public override bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public override long GetFileLength(string path)
        {
            return new FileInfo(path).Length;
        }

        public override byte[] ReadFileAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public override string ReadAllText(string path, Encoding encoding)
        {
            return File.ReadAllText(path, encoding);
        }

        public override Stream OpenFileRead(string path)
        {
            return File.OpenRead(path);
        }

        public override Stream CreateFile(string path)
        {
            return File.Create(path);
        }

        public override void DeleteFile(string path)
        {
            File.Delete(path);
        }



        public override bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public override bool CreateDirectoryIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }

        public override string[] GetDirectoryFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }



        public override void DownloadFileFromStorage(string storageFilePath, string localFilePath)
        {
            File.Copy(storageFilePath, localFilePath);
        }

        public override void UploadFileToStorage(string localFilePath, string storageFilePath)
        {
            File.Copy(localFilePath, storageFilePath);
        }

    }
}
