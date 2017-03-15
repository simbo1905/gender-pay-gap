using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Core;
using GenderPayGap.Core.Interfaces;

namespace GenderPayGap.Core.Classes
{
    public class Logger
    {
        public Logger(IFileRepository repository, string filePath, string instanceName=null)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            _repository = repository;

            if (string.IsNullOrWhiteSpace(Path.GetExtension(filePath)))
                filePath =Path.ChangeExtension(filePath, ".log");

            var filePath1 = filePath;
            _prefix = Path.GetFileNameWithoutExtension(filePath);
            _extension = Path.GetExtension(filePath);

            Directory = !string.IsNullOrWhiteSpace(instanceName) ? Path.Combine(Path.GetDirectoryName(filePath1), instanceName) : Path.GetDirectoryName(filePath1);

            _repository.CreateDirectory(Directory);
        }

        private readonly IFileRepository _repository;
        public readonly string Directory;
        private readonly object _syncRoot = new object();

        private readonly string _prefix;
        private readonly string _extension;

        private string DailyPath => Path.Combine(Directory, _prefix + "_" + DateTime.Now.ToString("yyMMdd") + _extension);

        void AppendToLog(string appendString)
        {
            if (string.IsNullOrWhiteSpace(appendString)) return;
            lock (_syncRoot)
            {
                _repository.Write(DailyPath, new []{appendString});
            }
        }

        public void WriteLine(string appendString, bool addPrefix=true)
        {
            if (string.IsNullOrWhiteSpace(appendString)) return;
            var prefix=$"Date:{DateTime.Now},Machine:{Environment.MachineName}";
            var instance = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
            if (!string.IsNullOrWhiteSpace(instance)) prefix += $",Instance:{ instance}";

            if (addPrefix) appendString = prefix + " -------\n" + appendString;
            AppendToLog(appendString + Environment.NewLine);
        }
    }
}
