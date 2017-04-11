using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Extensions;

namespace FreshSkies.mkryptor
{
    public sealed class TemporaryFile : IDisposable
    {
        public TemporaryFile(string directory=null, string fileName=null, string extension=null, bool createFile=true, bool forceDelete=true)
        {
            if (string.IsNullOrWhiteSpace(directory)) directory = TemporaryDirectory.DefaultDirectory;
            this.Directory = directory;
            this.ForceDelete = forceDelete;
            if (forceDelete || string.IsNullOrWhiteSpace(fileName))
                FullName = GetNewName(directory, fileName, extension);
            else
                FullName = Path.Combine(directory, fileName);

            if (createFile) Create();
        }

        ~TemporaryFile()
        {
            Delete();
        }

        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }

        public string FullName { get; private set; }

        public readonly string Directory;
        public string Name
        {
            get
            {
                return Path.GetFileName(FullName);
            }
        }
        public readonly bool ForceDelete=true;

        private void Create()
        {
            if (File.Exists(FullName)) File.Delete(FullName);
            using (File.Create(FullName)) { };
        }

        public static string GetNewName(string directory=null, string fileName=null, string extension=null)
        {
            if (string.IsNullOrWhiteSpace(directory)) directory = TemporaryDirectory.DefaultDirectory;
            string name = null;
            string fullName = null;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                do
                {
                    name = Path.GetRandomFileName();
                    if (!string.IsNullOrWhiteSpace(extension)) name = Path.ChangeExtension(name, extension);
                    fullName = Path.Combine(directory, name);
                }
                while (File.Exists(fullName));
            }
            else
            {
                fullName = Path.Combine(directory, fileName);
                extension = Path.GetExtension(fileName).TrimStart(' ', '.');
                fileName = Path.GetFileNameWithoutExtension(fileName);

                var c = 1;
                while (File.Exists(fullName))
                {
                    name = fileName + " " + c.ToString("000");
                    if (!string.IsNullOrWhiteSpace(extension)) name += "." + extension;
                    fullName = Path.Combine(directory, name);
                }
            }

            return fullName;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private void Delete()
        {
            if (FullName == null) return;

            try
            {
                File.Delete(FullName);
            }
            catch (Exception ex)
            {
                if (ForceDelete) throw;
            }
            FullName = null;
        }
    }
}
