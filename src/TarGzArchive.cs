using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    internal class TarGzArchive: IDisposable
    {

        private TarArchive _tarArchive;

        private string _tempFilePath;

        private string _destinationPath;

        private System.IO.Compression.ZipArchiveMode _archiveMode;
        
        private bool disposedValue;
        private CompressionLevel CompressionLevel;

        private TarGzArchive(string destinationPath, string tempFilePath, TarArchive tarArchive, System.IO.Compression.ZipArchiveMode archiveMode)
        {
            _destinationPath = destinationPath;
            _tempFilePath = tempFilePath;
            _archiveMode = archiveMode;
            _tarArchive = tarArchive;
        }

        public static TarGzArchive Create(string destinationPath)
        {
            //Get temp file 
            string tempFileName = Path.GetTempFileName();

            //Archive mode
            var zipArchiveMode = System.IO.Compression.ZipArchiveMode.Create;

            //Make tar archive
            var tarArchive = TarArchive.Create(tempFileName);
            
            return new TarGzArchive(destinationPath, tempFileName, tarArchive, zipArchiveMode);
        }

        public static TarGzArchive OpenForUpdating(string destinationPath)
        {
            //Get temp file 
            string tempFileName = Path.GetTempFileName();

            //Decompress destinationPath to tempFile
            var archiveStream = new System.IO.FileStream(destinationPath, FileMode.Open, FileAccess.Read, FileShare.None);
            var tempFileStream = new System.IO.FileStream(tempFileName, FileMode.Create, FileAccess.Write, FileShare.None);
            archiveStream.CopyTo(tempFileStream);

            //Close the streams
            archiveStream.Dispose();
            tempFileStream.Dispose();

            //Delete original gzipped file
            System.IO.File.Delete(destinationPath);

            //Archive mode
            var zipArchiveMode = System.IO.Compression.ZipArchiveMode.Update;

            //Make tar archive
            var tarArchive = TarArchive.OpenForUpdating(tempFileName);

            return new TarGzArchive(destinationPath, tempFileName, tarArchive, zipArchiveMode);
        }

        public void AddItem(EntryRecord entryRecord)
        {
            //Add file to tar archive
            _tarArchive.AddItem(entryRecord);
        }

        public void Compress()
        {
            _tarArchive.Dispose();

            //Open a stream to the temp file
            using var tempFileStream = new System.IO.FileStream(_tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            //Make the temp file a gz file
            int bufferSize = 4096;
            using System.IO.FileStream compressedFileStream = new FileStream(_destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize);
            using var compressor = new System.IO.Compression.GZipStream(compressedFileStream, CompressionLevel);
            tempFileStream.CopyTo(compressor);
            //compressor.Flush();
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
                    
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TarGzArchive()
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
