using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    internal class TarArchive: IDisposable
    {
        public System.IO.FileStream _archiveStream { get; }

        private System.Formats.Tar.TarWriter _tarWriter;

        private System.Formats.Tar.TarReader? _tarReader;

        private System.IO.Compression.ZipArchiveMode _archiveMode;

        private bool disposedValue;

        private TarArchive(FileStream archiveStream, TarWriter tarWriter, TarReader? tarReader, ZipArchiveMode archiveMode)
        {
            _archiveStream = archiveStream;
            _tarWriter = tarWriter;
            disposedValue = false;
            _tarReader = tarReader;
            _archiveMode = archiveMode;
        }

        public static TarArchive Create(string destinationPath)
        {
            System.IO.FileStream archiveStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            System.Formats.Tar.TarWriter tarWriter = new System.Formats.Tar.TarWriter(archiveStream, System.Formats.Tar.TarFormat.Gnu, true);
            return new TarArchive(archiveStream, tarWriter, null, ZipArchiveMode.Create);
        }

        public static TarArchive Create(System.IO.FileStream archiveStream)
        {
            System.Formats.Tar.TarWriter tarWriter = new System.Formats.Tar.TarWriter(archiveStream, System.Formats.Tar.TarFormat.Gnu, false);
            return new TarArchive(archiveStream, tarWriter, null, ZipArchiveMode.Create);
        }

        public static TarArchive OpenForUpdating(string destinationPath)
        {
            System.IO.FileStream archiveStream = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            System.Formats.Tar.TarWriter tarWriter = new System.Formats.Tar.TarWriter(archiveStream, System.Formats.Tar.TarFormat.Gnu, false);
            System.Formats.Tar.TarReader tarReader = new TarReader(archiveStream, true);
            return new TarArchive(archiveStream, tarWriter, tarReader, ZipArchiveMode.Update);
        }

        public void AddItem(EntryRecord entryRecord)
        {
            string entryName = entryRecord.Name.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            //bool hasEntryInArchive = _tarReader.
            _tarWriter.WriteEntry(entryRecord.FullPath, entryName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (_tarReader is not null) _tarReader.Dispose();
                    if (_tarWriter is not null) _tarWriter.Dispose();
                    if (_archiveStream is not null) _archiveStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TarArchive()
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
