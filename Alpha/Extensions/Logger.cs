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

        public void WriteLine(string appendString, bool timestamp=true)
        {
            if (string.IsNullOrWhiteSpace(appendString)) return;
            if (timestamp) appendString = DateTime.Now.ToLongTimeString() + " ------- " + appendString;
            AppendToLog(appendString + Environment.NewLine);
        }
    }
}
