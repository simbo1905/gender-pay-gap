using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Extensions;

    public sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory() : this(Path.GetTempPath()) { }

        public TemporaryDirectory(string parentDirectory)
        {
            var fullName = GetNewName(parentDirectory);

            this.Directory = System.IO.Directory.CreateDirectory(fullName);
        }

        ~TemporaryDirectory()
        {
            Delete();
        }

        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }

        static string _DefaultDirectory;
        public static string DefaultDirectory
        {
            get
            {
                if (_DefaultDirectory != null) return _DefaultDirectory;
                return FileSystem.GetTempPath();
            }
            set
            {
                _DefaultDirectory=value;
            }
        }

        public readonly DirectoryInfo  Directory;

        public string FullName
        {
            get
            {
                return Directory.FullName;
            }
        }

        private void Delete()
        {
            Directory.Delete(true);
        }

        public static string GetNewName(string parentDirectory=null)
        {
            if (string.IsNullOrWhiteSpace(parentDirectory)) parentDirectory = DefaultDirectory;
            string fullName = null;
            do
            {
                var subDir = Path.GetRandomFileName();
                Path.ChangeExtension(subDir, "tmp");
                fullName = Path.Combine(parentDirectory, subDir);
            }
            while (System.IO.Directory.Exists(fullName));
            return fullName;
        }
    }
