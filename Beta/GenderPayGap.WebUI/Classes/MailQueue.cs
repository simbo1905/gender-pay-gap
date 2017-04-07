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
        public MailQueue(IFileRepository repository, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
            
             _repository = repository;

            _prefix = Path.GetFileNameWithoutExtension(path);

            Directory = Path.GetDirectoryName(path);

            _repository.CreateDirectory(Directory);
        }

        private readonly IFileRepository _repository;
        public readonly string Directory;
        private readonly object _syncRoot = new object();

        private readonly string _prefix;
        private readonly string _extension;

        private string DailyPath => Path.Combine(Directory, DateTime.Now.ToString("yyyy-MM-dd"));

        void AddFile(byte[] bytes, string filename=null)
        {
            lock (_syncRoot)
            {
                var filePath = Path.Combine(DailyPath, filename);

                _repository.Write(DailyPath, bytes);
            }
        }
    }
}
