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

        private TarGzArchive(string destinationPath, string tempFilePath, TarArchive tarArchive, System.IO.Compression.ZipArchiveMode archiveMode, CompressionLevel compressionLevel)
        {
            _destinationPath = destinationPath;
            _tempFilePath = tempFilePath;
            _archiveMode = archiveMode;
            _tarArchive = tarArchive;
            CompressionLevel = compressionLevel;
        }

        public static TarGzArchive Create(string destinationPath, CompressionLevel compressionLevel)
        {
            //Get temp file 
            string tempFileName = Path.GetTempFileName();

            //Archive mode
            var zipArchiveMode = System.IO.Compression.ZipArchiveMode.Create;

            //Make tar archive
            var tarArchive = TarArchive.Create(tempFileName);
            
            return new TarGzArchive(destinationPath, tempFileName, tarArchive, zipArchiveMode, compressionLevel);
        }

        public static TarGzArchive OpenForUpdating(string destinationPath, CompressionLevel compressionLevel)
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
            //Note: we should only delete the file at the end, but we are doing it here for now
            System.IO.File.Delete(destinationPath);

            //Archive mode
            var zipArchiveMode = System.IO.Compression.ZipArchiveMode.Update;

            //Make tar archive
            var tarArchive = TarArchive.OpenForUpdating(tempFileName);

            return new TarGzArchive(destinationPath, tempFileName, tarArchive, zipArchiveMode, compressionLevel);
        }

        public static TarGzArchive OpenForReading(string archivePath)
        {
            //Get temp file 
            string tempFileName = Path.GetTempFileName();

            //Decompress archive to the temp file
            Decompress(tempFileName, archivePath);

            //Archive mode
            var zipArchiveMode = System.IO.Compression.ZipArchiveMode.Read;

            //Make tar archive
            var tarArchive = TarArchive.OpenForReading(tempFileName);

            return new TarGzArchive(archivePath, tempFileName, tarArchive, zipArchiveMode, CompressionLevel.NoCompression);
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
            using System.IO.FileStream compressedFileStream = new FileStream(_destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var compressor = new System.IO.Compression.GZipStream(compressedFileStream, CompressionLevel);
            tempFileStream.CopyTo(compressor, bufferSize);
            //compressor.Flush();
        }

        //Decompresses tar.gz archive to a temp file
        public static void Decompress(string tempFilePath, string archivePath)
        {
            //Open a stream to the temp file
            using System.IO.FileStream tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

            //Make the temp file a gz file
            int bufferSize = 4096;
            using var archiveFileStream = new System.IO.FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var compressor = new System.IO.Compression.GZipStream(archiveFileStream, CompressionMode.Decompress);
            compressor.CopyTo(tempFileStream, bufferSize);
        }

        public void ExpandArchive(string destinationPath, bool overwrite, string filter)
        {
            _tarArchive.ExpandArchive(destinationPath, overwrite, filter);
        }

        public bool HasOneTopLevelEntries()
        {
            return _tarArchive.HasOneTopLevelEntries();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _tarArchive.Dispose();
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
