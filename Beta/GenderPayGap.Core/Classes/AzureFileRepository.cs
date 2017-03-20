using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Extensions;
using GenderPayGap.Core.Interfaces;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.File;

namespace GenderPayGap.Core.Classes
{
    public class AzureFileRepository:IFileRepository
    {
        public AzureFileRepository(string connectionString, string shareName)
        {
            // Parse the connection string and return a reference to the storage account.
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a CloudFileClient object for credentialed access to File storage.
            var fileClient = storageAccount.CreateCloudFileClient();

            var share = fileClient.GetShareReference(shareName);

            _rootDir = share.GetRootDirectoryReference();
        }

        private readonly CloudFileDirectory _rootDir;


        private CloudFileDirectory GetDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));
            directoryPath = directoryPath.TrimI(@"/\");
            var dirs = directoryPath.SplitI(@"/\");
            if (dirs.Length < 1) return _rootDir;

            var directory = _rootDir;

            for (var i=0;i<dirs.Length; i++)
            {
                var file = directory.GetFileReference(dirs[i]);
                if (file != null && file.Exists()) return null;
                directory = directory.GetDirectoryReference(dirs[i]);
                if (directory == null || !directory.Exists()) return null;
            }
            return directory;
        }

        private CloudFile GetFile(string filePath)
        {
            var directory = GetDirectory(Path.GetDirectoryName(filePath));
            return directory?.GetFileReference(Path.GetFileName(filePath));
        }

        public void CreateDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));
            directoryPath = directoryPath.TrimI(@"/\");
            var dirs = directoryPath.SplitI(@"/\");
            if (dirs.Length < 1) return;

            var directory = _rootDir;

            for (var i = 0; i < dirs.Length; i++)
            {
                var file = directory.GetFileReference(dirs[i]);
                if (file != null && file.Exists()) return;
                directory = directory.GetDirectoryReference(dirs[i]);
                if (directory != null && !directory.Exists())
                    directory.Create();
            }
        }

        public void DeleteFiles(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));

            foreach (var file in GetFiles(directoryPath))
                DeleteFile(file);
        }

        public bool GetDirectoryExists(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));
            return GetDirectory(directoryPath)!=null;
        }

        public bool GetFileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))throw new ArgumentNullException(nameof(filePath));
            var directory = GetDirectory(Path.GetDirectoryName(filePath));
            if (directory == null) return false;
            return GetFile(filePath)!=null;
        }

        public DateTime GetLastWriteTime(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            var file = GetFile(filePath);
            if (file==null || !file.Exists())throw new FileNotFoundException($"Cannot find file '{filePath}'");
            return file.Properties.LastModified.Value.LocalDateTime;
        }

        public DateTime GetLastAccessTime(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            var file = GetFile(filePath);
            if (file == null) throw new FileNotFoundException($"Cannot find file '{filePath}'");
            throw new NotImplementedException();
        }

        public long GetFileSize(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            var file = GetFile(filePath);
            if (file == null) throw new FileNotFoundException($"Cannot find file '{filePath}'");
            file.FetchAttributes();
            return file.Properties.Length;
        }


        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            var file = GetFile(filePath);
            file?.Delete();
        }

        public void RenameFile(string filePath, string newFilename)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrWhiteSpace(newFilename)) throw new ArgumentNullException(nameof(newFilename));

            var directory = GetDirectory(Path.GetDirectoryName(filePath));
            if (directory == null) throw new FileNotFoundException($"Cannot find file '{filePath}'");

            var file=directory.GetFileReference(Path.GetFileName(filePath));
            if (file == null) throw new FileNotFoundException($"Cannot find file '{filePath}'");
            var newfile = directory.GetFileReference(newFilename);
            if (newfile!=null)throw new IOException($"The destination file '{newFilename}' already exists");

            var fileCopy = directory.GetFileReference(newFilename);
            fileCopy.StartCopy(file);
            file.DeleteIfExists();
        }

        public IEnumerable<string> GetFiles(string directoryPath, string searchPattern = null)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));
            var directory = GetDirectory(directoryPath);
            if (directory==null)throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");

            var files = !string.IsNullOrWhiteSpace(searchPattern) ? directory.ListFilesAndDirectories().OfType<CloudFile>().Where(f => f.Name.Like(searchPattern)) : directory.ListFilesAndDirectories().OfType<CloudFile>();

            return files.Select(file => Path.Combine(directoryPath, file.Name));
        }

        public string Read(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            var file = GetFile(filePath);
            if (file == null) throw new FileNotFoundException($"Cannot find file '{filePath}'");
            return file.DownloadText();
        }

        public void Write(string filePath, IEnumerable<string> lines)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            var text = lines.ToDelimitedString(Environment.NewLine);

            var file = GetFile(filePath);
            if (file.Exists())
            {
                var buffer = Encoding.UTF8.GetBytes(text);
                file.Resize(file.Properties.Length + buffer.Length);
                using (var fileStream = file.OpenWrite(null))
                {
                    fileStream.Seek(buffer.Length * -1, SeekOrigin.End);
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
            else
                file.UploadText(text);
        }

        public void Write(string filePath, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            var file = GetFile(filePath);

            file.UploadFromStream(stream);
        }

        public void Write(string filePath, FileInfo uploadFile)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!uploadFile.Exists) throw new FileNotFoundException(nameof(uploadFile));
            var file = GetFile(filePath);

            file.UploadFromFile(uploadFile.FullName);
        }

        public string GetFullPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.Name, filePath);
            return filePath;
        }

    }
}
