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

        private string? _sourceDirectoryFullPath;

        private string _destinationPath;

        private System.IO.Compression.ZipArchive _zipArchive;

        private System.IO.FileStream _archiveFileStream;

        private bool disposedValue;

        public ZipArchive(string? sourceDirectoryFullPath, string destinationPath, System.IO.FileStream archiveFileStream, System.IO.Compression.ZipArchive zipArchive)
        {
            _sourceDirectoryFullPath = sourceDirectoryFullPath;
            _destinationPath = destinationPath;
            _zipArchive = zipArchive;
            _archiveFileStream = archiveFileStream;
        }

        public static ZipArchive Create(string? sourceDirectoryFullPath, string destinationPath)
        {
            //Create stream
            System.IO.FileStream archiveStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            var zipArchive = new System.IO.Compression.ZipArchive(archiveStream, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: false);
            //zipArchive.Dispose();

            //zipArchive = new System.IO.Compression.ZipArchive(archiveStream, System.IO.Compression.ZipArchiveMode.C, leaveOpen: false);


            ZipArchive archive = new ZipArchive(sourceDirectoryFullPath, destinationPath, archiveStream, zipArchive);
            return archive;
        }

        public void AddFile(string fileFullPath)
        {
            var entryName = (_sourceDirectoryFullPath is not null) ? fileFullPath.Replace(_sourceDirectoryFullPath, "").Trim() : System.IO.Path.GetFileName(fileFullPath);
            entryName = entryName.Replace(System.IO.Path.PathSeparator, System.IO.Path.AltDirectorySeparatorChar);

            _zipArchive.CreateEntryFromFile(fileFullPath, entryName, CompressionLevel.Optimal);
        }

        public void AddItem(string itemFullPath)
        {
            if (System.IO.File.Exists(itemFullPath))
            {
                AddFile(itemFullPath);
            } else
            {
                AddDirectory(itemFullPath);
            }
        }

        public void AddDirectory(string directoryFullPath)
        {
            System.IO.DirectoryInfo info = new DirectoryInfo(directoryFullPath);


            var subfiles = info.GetFiles();
            var subdirectories = info.GetDirectories();
            if (subfiles.Length + subdirectories.Length == 0)
            {
                var entryName = (_sourceDirectoryFullPath is not null) ? directoryFullPath.Replace(_sourceDirectoryFullPath, "").Trim() : System.IO.Path.GetFileName(directoryFullPath);
                entryName = entryName.Replace(System.IO.Path.PathSeparator, System.IO.Path.AltDirectorySeparatorChar);
                entryName += System.IO.Path.AltDirectorySeparatorChar;
                _zipArchive.CreateEntry(entryName);
                return;
            }

        }

        public void AddFileLegacy(string fileFullPath)
        {
            //Assume fileFullPath = _sourceDirectoryFullPath + _relativePath
            var relativePath = (_sourceDirectoryFullPath is not null) ? fileFullPath.Replace(_sourceDirectoryFullPath, "").Trim() : System.IO.Path.GetFileName(fileFullPath);

            var sourceFileStream = System.IO.File.Open(fileFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var sourceBinaryStream = new System.IO.BinaryReader(sourceFileStream);

            var entryPath = relativePath.Replace(System.IO.Path.PathSeparator, System.IO.Path.AltDirectorySeparatorChar);

            //Create entry in the archive
            var entry = _zipArchive.CreateEntry(entryPath, System.IO.Compression.CompressionLevel.Optimal);

            var archiveEntryStream = entry.Open();
            var archiveEntryBinaryWriter = new BinaryWriter(archiveEntryStream);

            //4KB buffer
            var buffer = new byte[1024 * 4];
            int bytesRead = sourceBinaryStream.Read(buffer, 0, buffer.Length);
            while (bytesRead > 0)
            {
                archiveEntryBinaryWriter.Write(buffer, 0, bytesRead);
                archiveEntryBinaryWriter.Flush();
                bytesRead = sourceBinaryStream.Read(buffer, 0, buffer.Length);
            }
            archiveEntryBinaryWriter.Dispose();
            archiveEntryStream.Dispose();
            sourceBinaryStream.Dispose();
            sourceFileStream.Dispose();
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
