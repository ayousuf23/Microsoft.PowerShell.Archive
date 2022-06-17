using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    internal class ZipArchive : IDisposable
    {

        private string _destinationPath;

        private System.IO.Compression.ZipArchive _zipArchive;

        private System.IO.FileStream _archiveFileStream;

        private bool disposedValue;

        public ZipArchive(string destinationPath, System.IO.FileStream archiveFileStream, System.IO.Compression.ZipArchive zipArchive)
        {
            _destinationPath = destinationPath;
            _zipArchive = zipArchive;
            _archiveFileStream = archiveFileStream;
        }

        public static ZipArchive Create(string destinationPath)
        {
            //Create stream
            System.IO.FileStream archiveStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            var zipArchive = new System.IO.Compression.ZipArchive(archiveStream, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: false);


            ZipArchive archive = new ZipArchive(destinationPath, archiveStream, zipArchive);
            return archive;
        }

        public void AddFile(string fileFullPath, string? sourceDirectoryFullPath)
        {
            var entryName = (sourceDirectoryFullPath is not null) ? fileFullPath.Replace(sourceDirectoryFullPath, "").Trim() : System.IO.Path.GetFileName(fileFullPath);
            entryName = entryName.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

            _zipArchive.CreateEntryFromFile(fileFullPath, entryName, CompressionLevel.Optimal);
        }

        public void AddItem(string itemFullPath)
        {
            if (System.IO.File.Exists(itemFullPath))
            {
                AddFile(itemFullPath, null);
            } else
            {
                AddDirectory(itemFullPath, null);
            }
        }

        public void AddDirectory(string directoryFullPath, string? sourceDirectoryFullPath)
        {
            System.IO.DirectoryInfo info = new DirectoryInfo(directoryFullPath);


            var subfiles = info.GetFiles();
            var subdirectories = info.GetDirectories();
            if (subfiles.Length + subdirectories.Length == 0)
            {
                var entryName = (sourceDirectoryFullPath is not null) ? directoryFullPath.Replace(sourceDirectoryFullPath, "").Trim() : System.IO.Path.GetFileName(directoryFullPath);
                entryName = entryName.Replace(System.IO.Path.PathSeparator, System.IO.Path.AltDirectorySeparatorChar);
                entryName += System.IO.Path.AltDirectorySeparatorChar;
                _zipArchive.CreateEntry(entryName);
                return;
            }

            //Add subfiles and subdirectories

            if (sourceDirectoryFullPath is null)
            {
                sourceDirectoryFullPath = info.Parent.FullName;
                if (!sourceDirectoryFullPath.EndsWith(System.IO.Path.DirectorySeparatorChar)) sourceDirectoryFullPath += System.IO.Path.DirectorySeparatorChar.ToString();
            }
                foreach (var subfile in subfiles)
            {
                AddFile(subfile.FullName, sourceDirectoryFullPath);
            }

            foreach (var directory in subdirectories)
            {
                AddDirectory(directory.FullName, sourceDirectoryFullPath);
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _zipArchive.Dispose();
                    _archiveFileStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ZipArchive()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
