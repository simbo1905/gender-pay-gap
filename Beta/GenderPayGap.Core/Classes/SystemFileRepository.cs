using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using GenderPayGap.Core.Interfaces;
using Microsoft.WindowsAzure.Storage;

namespace GenderPayGap.Core.Classes
{
    public class SystemFileRepository:IFileRepository
    {

        public SystemFileRepository(string rootPath=null)
        {
            rootPath = string.IsNullOrWhiteSpace(rootPath) ? AppDomain.CurrentDomain.BaseDirectory : FileSystem.ExpandLocalPath(rootPath);
            _rootDir = new DirectoryInfo(rootPath);
        }

        private DirectoryInfo _rootDir = null;

        public bool GetFileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            return System.IO.File.Exists(filePath);
        }

        public void CreateDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));
            if (!Path.IsPathRooted(directoryPath)) directoryPath = Path.Combine(_rootDir.FullName, directoryPath);

            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch
            {

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
            if (!Path.IsPathRooted(directoryPath)) directoryPath = Path.Combine(_rootDir.FullName, directoryPath);

            return System.IO.Directory.Exists(directoryPath);
        }

        public DateTime GetLastWriteTime(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);

            return System.IO.File.GetLastWriteTime(filePath);
        }

        public DateTime GetLastAccessTime(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            return System.IO.File.GetLastAccessTime(filePath);
        }

        public long GetFileSize(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            return new FileInfo(filePath).Length;
        }

        public void AppendLines(string filePath, IEnumerable<string> lines)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            File.AppendAllLines(filePath,lines);
        }

        public void DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            if (File.Exists(filePath))File.Delete(filePath);
        }

        public void RenameFile(string filePath, string newFilename)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrWhiteSpace(newFilename)) throw new ArgumentNullException(nameof(newFilename));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            File.Move(filePath,newFilename);
        }

        public IEnumerable<string> GetFiles(string directoryPath, string searchPattern = null)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ArgumentNullException(nameof(directoryPath));
            if (!Path.IsPathRooted(directoryPath)) directoryPath = Path.Combine(_rootDir.FullName, directoryPath);
            if (!Directory.Exists(directoryPath))throw new DirectoryNotFoundException($"Cannot find directory '{directoryPath}'");

            return string.IsNullOrWhiteSpace(searchPattern) ? Directory.GetFiles(directoryPath) : Directory.GetFiles(directoryPath, searchPattern);
        }

        public string Read(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            return File.ReadAllText(filePath);
        }

        public void Write(string filePath, IEnumerable<string> lines)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            File.AppendAllLines(filePath,lines);
        }

        public void Write(string filePath, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            File.AppendAllText(filePath, stream.ToString());
        }

        public void Write(string filePath, FileInfo uploadFile)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            if (!uploadFile.Exists) throw new FileNotFoundException(nameof(uploadFile));
            File.AppendAllText(filePath, File.ReadAllText(uploadFile.FullName));
        }

        public string GetFullPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!Path.IsPathRooted(filePath)) filePath = Path.Combine(_rootDir.FullName, filePath);
            return filePath;
        }
    }
}
