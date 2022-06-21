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
        private System.IO.FileStream _archiveStream;

        private System.Formats.Tar.TarWriter _tarWriter;
        
        private bool disposedValue;

        private TarArchive(FileStream archiveStream, TarWriter tarWriter)
        {
            _archiveStream = archiveStream;
            _tarWriter = tarWriter;
            disposedValue = false;
        }

        public static TarArchive Create(string destinationPath)
        {
            System.IO.FileStream archiveStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            System.Formats.Tar.TarWriter tarWriter = new System.Formats.Tar.TarWriter(archiveStream, System.Formats.Tar.TarFormat.Gnu, false);
            return new TarArchive(archiveStream, tarWriter);
        }

        public void AddItem(EntryRecord entryRecord)
        {
            string entryName = entryRecord.Name.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            _tarWriter.WriteEntry(entryRecord.FullPath, entryName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _tarWriter.Dispose();
                    _archiveStream.Dispose();
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
