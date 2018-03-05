using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServer.Storage.LayeredStorage
{
    public class LayeredStorageCommand : IStorageCommand
    {
        private LayeredStorageConnection _connection;
        private IStorageConnection[] _layers;
        private IStorageCommand[] _layerCommands;

        public LayeredStorageCommand(LayeredStorageConnection connection)
        {
            _connection = connection;
            _layers = _connection.Layers;

            var commands = new List<IStorageCommand>();
            foreach(var layer in _layers)
            {
                commands.Add(layer.CreateCommand());
            }
            _layerCommands = commands.ToArray();
        }

        public IStorageConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        private IStorageCommand FindStorageCommandForFile(string filePath)
        {
            foreach(var command in _layerCommands)
            {
                if (command.FileExists(filePath))
                {
                    return command;
                }
            }
            return null;
        }

        private IStorageCommand FindStorageCommandForDirectory(string DirectoryPath)
        {
            foreach (var command in _layerCommands)
            {
                if (command.DirectoryExists(DirectoryPath))
                {
                    return command;
                }
            }
            return null;
        }

        private IStorageCommand CopyFileToTopLayer(string path)
        {
            var command = FindStorageCommandForFile(path);
            if (command != null)
            {
                var topLayerCommand = _layerCommands[0];
                if (command != topLayerCommand)
                {
                    using (var sourceStream = command.OpenFileRead(path))
                    {
                        using (var destStream = topLayerCommand.CreateFile(path))
                        {
                            sourceStream.CopyTo(destStream);
                        }
                    }
                }
                return topLayerCommand;
            }
            return null;
        }

        public void CopyFile(string sourceFileName, string destFileName)
        {
            var sourceCommand = FindStorageCommandForFile(sourceFileName);
            var topLayerCommand = _layerCommands[0];
            using (var sourceStream = sourceCommand.OpenFileRead(sourceFileName))
            {
                using (var destStream = topLayerCommand.CreateFile(destFileName))
                {
                    sourceStream.CopyTo(destStream);
                }
            }
        }

        public void MoveFile(string sourceFileName, string destFileName)
        {
            var sourceCommand = FindStorageCommandForFile(sourceFileName);
            var topLayerCommand = _layerCommands[0];
            using (var sourceStream = sourceCommand.OpenFileRead(sourceFileName))
            {
                using (var destStream = topLayerCommand.CreateFile(destFileName))
                {
                    sourceStream.CopyTo(destStream);
                }
            }
            sourceCommand.DeleteFile(sourceFileName);
        }

        public bool FileExists(string path)
        {
            var command = CopyFileToTopLayer(path);
            return (command != null);
        }

        public long GetFileLength(string path)
        {
            var command = CopyFileToTopLayer(path);
            if (command != null)
            {
                return command.GetFileLength(path);
            }
            throw new Exception("Cannot find file " + path);
        }

        public byte[] ReadAllBytes(string path)
        {
            var command = CopyFileToTopLayer(path);
            if (command != null)
            {
                return command.ReadAllBytes(path);
            }
            throw new Exception("Cannot find file " + path);
        }

        public string ReadAllText(string path)
        {
            var command = CopyFileToTopLayer(path);
            if (command != null)
            {
                return command.ReadAllText(path);
            }
            throw new Exception("Cannot find file " + path);
        }

        public string ReadAllText(string path, Encoding encoding)
        {
            var command = CopyFileToTopLayer(path);
            if (command != null)
            {
                return command.ReadAllText(path, encoding);
            }
            throw new Exception("Cannot find file " + path);
        }

        public Stream OpenFileRead(string path)
        {
            var command = CopyFileToTopLayer(path);
            if (command != null)
            {
                return command.OpenFileRead(path);
            }
            throw new Exception("Cannot find file " + path);
        }

        public Stream CreateFile(string path)
        {
            var command = _layerCommands[0];
            return command.CreateFile(path);
        }

        public void DeleteFile(string path)
        {
            foreach (var command in _layerCommands)
            {
                if (command.FileExists(path))
                {
                    command.DeleteFile(path);
                }
            }
        }

        public string GetRawAbsolutePath(string path)
        {
            var command = CopyFileToTopLayer(path);
            if (command == null)
            {
                command = _layerCommands[0];
            }
            return command.GetRawAbsolutePath(path);
        }



        public bool DirectoryExists(string path)
        {
            var command = FindStorageCommandForDirectory(path);
            return command != null;
        }

        public bool CreateDirectoryIfNotExist(string path)
        {
            var command = _layerCommands[0];
            return command.CreateDirectoryIfNotExist(path);
        }

        public string[] GetDirectoryFiles(string path, string searchPattern)
        {
            var files = new List<string>();
            foreach (var command in _layerCommands)
            {
                var result = command.GetDirectoryFiles(path, searchPattern);
                if ((result != null) && (result.Length > 0))
                {
                    files.AddRange(result);
                }
            }
            return files.ToArray();
        }



        public void DownloadFileFromStorage(string storageFilePath, string localFilePath)
        {
            var command = CopyFileToTopLayer(storageFilePath);
            if (command != null)
            {
                command.DownloadFileFromStorage(storageFilePath, localFilePath);
            }
            else 
            {
                throw new Exception("Cannot find file " + storageFilePath);
            }
        }

        public void UploadFileToStorage(string localFilePath, string storageFilePath)
        {
            var topLayerCommand = _layerCommands[0];
            topLayerCommand.UploadFileToStorage(localFilePath, storageFilePath);
        }

        ~LayeredStorageCommand()
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
                foreach (var command in _layerCommands)
                {
                    command.Dispose();
                }
                _layerCommands = null;
            }
        }

    }
}
