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

        private System.IO.Compression.ZipArchiveMode _archiveMode;

        private bool disposedValue;

        public System.IO.Compression.CompressionLevel CompressionLevel { get; set; }

        public ZipArchive(string destinationPath, System.IO.FileStream archiveFileStream, System.IO.Compression.ZipArchive zipArchive, System.IO.Compression.ZipArchiveMode archiveMode)
        {
            _destinationPath = destinationPath;
            _zipArchive = zipArchive;
            _archiveFileStream = archiveFileStream;
            _archiveMode = archiveMode;
        }

        public static ZipArchive Create(string destinationPath)
        {
            //Create stream
            System.IO.FileStream archiveStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            var zipArchive = new System.IO.Compression.ZipArchive(archiveStream, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: false);


            ZipArchive archive = new ZipArchive(destinationPath, archiveStream, zipArchive, System.IO.Compression.ZipArchiveMode.Create);
            return archive;
        }

        public static ZipArchive OpenForUpdating(string destinationPath)
        {
            System.IO.FileStream archiveStream = new FileStream(destinationPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var zipArchive = new System.IO.Compression.ZipArchive(archiveStream, System.IO.Compression.ZipArchiveMode.Update, leaveOpen: false);

            ZipArchive archive = new ZipArchive(destinationPath, archiveStream, zipArchive, System.IO.Compression.ZipArchiveMode.Update);
            return archive;
        }

        public static ZipArchive OpenForReading(string archivePath)
        {
            System.IO.FileStream archiveStream = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var zipArchive = new System.IO.Compression.ZipArchive(archiveStream, ZipArchiveMode.Read, true);

            ZipArchive archive = new ZipArchive(archivePath, archiveStream, zipArchive, ZipArchiveMode.Read);
            return archive;
        }

        public void AddItem(EntryRecord entry)
        {
            string entryName = entry.Name.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            var entryInArchive = (_archiveMode == ZipArchiveMode.Create) ? null : _zipArchive.GetEntry(entryName);
            if (entryName.EndsWith(System.IO.Path.AltDirectorySeparatorChar))
            {
                //Just create an entry
                if (entryInArchive == null) _zipArchive.CreateEntry(entryName);
            } else
            {
                if (entryInArchive != null)
                {
                    entryInArchive.Delete();
                }
                _zipArchive.CreateEntryFromFile(entry.FullPath, entryName, CompressionLevel);
            }
        }

        public void SetCompressionLevel(string? compressionLevel)
        {
            if (compressionLevel == "Optimal") CompressionLevel = CompressionLevel.Optimal;
            else if (compressionLevel == "Fastest") CompressionLevel = CompressionLevel.Fastest;
            else CompressionLevel = CompressionLevel.NoCompression;
        }

        public void ExpandArchive(string destinationPath, bool overwrite)
        {
            foreach (var entry in _zipArchive.Entries)
            {
                string normalizedEntryName = entry.FullName.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
                string fileDestinationPath = destinationPath + normalizedEntryName;
                if (File.Exists(fileDestinationPath) && !overwrite)
                {
                    //Throw an error
                    System.InvalidOperationException exception = new InvalidOperationException($"The file {fileDestinationPath} already exists");
                    throw exception;
                }

                if (fileDestinationPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
                {
                    //If the directory does not exist create it
                    if (!System.IO.Directory.Exists(fileDestinationPath)) System.IO.Directory.CreateDirectory(fileDestinationPath);

                } else
                {
                    //Create the file
                    entry.ExtractToFile(fileDestinationPath, overwrite);
                }
                
            }
        }

        public bool HasOneTopLevelEntries()
        {
            var entryNames = _zipArchive.Entries.GroupBy(entry => entry.FullName).Select(entry => entry.Key).ToList();
            return entryNames.GroupBy(x => x.Split(System.IO.Path.AltDirectorySeparatorChar).First())
                       .Where(x => x.Count() > 0)
                       .Count() == 1;
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
