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

        public System.IO.Compression.CompressionLevel CompressionLevel { get; set; }

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

        public static ZipArchive OpenForUpdating(string destinationPath)
        {
            System.IO.FileStream archiveStream = new FileStream(destinationPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var zipArchive = new System.IO.Compression.ZipArchive(archiveStream, System.IO.Compression.ZipArchiveMode.Update, leaveOpen: false);

            ZipArchive archive = new ZipArchive(destinationPath, archiveStream, zipArchive);
            return archive;
        }

        public void AddItem(EntryRecord entry)
        {
            string entryName = entry.Name.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            if (entryName.EndsWith(System.IO.Path.AltDirectorySeparatorChar))
            {
                //Just create an entry
                _zipArchive.CreateEntry(entryName);
            } else
            {
                _zipArchive.CreateEntryFromFile(entry.FullPath, entryName, CompressionLevel);
            }
        }

        public void SetCompressionLevel(string? compressionLevel)
        {
            if (compressionLevel == "Optimal") CompressionLevel = CompressionLevel.Optimal;
            else if (compressionLevel == "Fastest") CompressionLevel = CompressionLevel.Fastest;
            else CompressionLevel = CompressionLevel.NoCompression;
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
