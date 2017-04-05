using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace GenderPayGap.Core.Interfaces
{
    public interface IFileRepository
    {
        void CreateDirectory(string directoryPath);

        void DeleteFiles(string directoryPath);

        bool GetDirectoryExists(string directoryPath);

        bool GetFileExists(string filePath);
        DateTime GetLastWriteTime(string filePath);
        DateTime GetLastAccessTime(string filePath);
        long GetFileSize(string filePath);

        void DeleteFile(string filePath);
        void RenameFile(string filePath,string newFilename);

        IEnumerable<string> GetFiles(string directoryPath, string searchPattern = null);

        string Read(string filePath);
        void Write(string filePath, string text);

        void Write(string filePath, IEnumerable<string> lines);
        void Write(string filePath, byte[] bytes);
        void Write(string filePath, Stream stream);
        void Write(string filePath, FileInfo uploadFile);

        string GetFullPath(string filePath);
    }

}
