using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Autofac;
using Autofac.Core;
using CsvHelper;
using Extensions;
using GenderPayGap.Core.Interfaces;

namespace GenderPayGap.Core.Classes
{
    public class MailQueue
    {
        public MailQueue(IFileRepository repository, string path, string extension=".eml")
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            
             _repository = repository;
            Extension = extension;
            Directory = path;

            _repository.CreateDirectory(Directory);
        }

        private readonly IFileRepository _repository;
        public readonly string Directory;
        public readonly string Extension;

        private readonly object _syncRoot = new object();

        private string DailyPath => Path.Combine(Directory, DateTime.Now.ToString("yyyyMMdd"));

        public void Enqueue(byte[] bytes,string extension=null)
        {
            if (bytes==null || bytes.Length==0) return;

            lock (_syncRoot)
            {
                string filepath;
                _repository.CreateDirectory(Path.Combine(DailyPath));
                if (string.IsNullOrWhiteSpace(extension)) extension = Extension;
                if (!extension.StartsWith(".")) extension = "."+ extension;
                do
                {
                    filepath = Path.Combine(DailyPath, $"{Guid.NewGuid().ToShortString()}{extension}");
                } while (_repository.GetFileExists(filepath));

                _repository.Write(filepath, bytes);
            }
        }
    }
}
