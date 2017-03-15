using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace Extensions
{
    public class Logger
    {
        public Logger(string filePath, string instanceName=null)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException("filePath");

            if (string.IsNullOrWhiteSpace(Path.GetExtension(filePath)))
                filePath =Path.ChangeExtension(filePath, ".log");

            FilePath = filePath;
            Prefix = Path.GetFileNameWithoutExtension(filePath);
            Extension = Path.GetExtension(filePath);
            if (!string.IsNullOrWhiteSpace(instanceName))
                Directory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(FilePath), instanceName));
            else
                Directory = new DirectoryInfo(Path.GetDirectoryName(FilePath));

            try
            {
                System.IO.Directory.CreateDirectory(Directory.FullName);
            }
            catch
            {
                
            }
        }

        private readonly string FilePath;
        public readonly DirectoryInfo Directory;
        private readonly object SyncRoot = new object();

        private readonly string Prefix;
        private readonly string Extension;

        private string DailyPath
        {
            get
            {
                return Path.Combine(Directory.FullName, Prefix + "_" + DateTime.Now.ToString("yyMMdd") + Extension);
            }
        }
        void AppendToLog(string appendString)
        {
            if (string.IsNullOrWhiteSpace(appendString)) return;
            lock (SyncRoot)
            {
                File.AppendAllText(DailyPath, appendString);
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
