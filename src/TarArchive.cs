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

        private System.Formats.Tar.TarWriter? _tarWriter;

        private System.Formats.Tar.TarReader? _tarReader;

        private System.IO.Compression.ZipArchiveMode _archiveMode;

        private bool disposedValue;

        private TarArchive(FileStream archiveStream, TarWriter? tarWriter, TarReader? tarReader, ZipArchiveMode archiveMode)
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
            System.IO.FileStream archiveStream = new FileStream(destinationPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            System.Formats.Tar.TarWriter tarWriter = new System.Formats.Tar.TarWriter(archiveStream, System.Formats.Tar.TarFormat.Gnu, true);
            System.Formats.Tar.TarReader tarReader = new TarReader(archiveStream, true);
            return new TarArchive(archiveStream, tarWriter, tarReader, ZipArchiveMode.Update);
        }

        public static TarArchive OpenForReading(string archivePath)
        {
            System.IO.FileStream archiveStream = new FileStream(archivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            System.Formats.Tar.TarReader tarReader = new TarReader(archiveStream, true);
            return new TarArchive(archiveStream, null, tarReader, ZipArchiveMode.Read);
        }

        public void AddItem(EntryRecord entryRecord)
        {
            string entryName = entryRecord.Name.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            //bool hasEntryInArchive = _tarReader.
            _tarWriter.WriteEntry(entryRecord.FullPath, entryName);
        }

        public void ExpandArchive(string destinationPath, bool overwrite, string filter)
        {
            System.Management.Automation.WildcardPattern wildcardPattern = new System.Management.Automation.WildcardPattern(filter);
            _archiveStream.Position = 0;
            _tarReader.Dispose();
            _tarReader = new TarReader(_archiveStream, false);
            var entry = _tarReader.GetNextEntry();
            while (entry != null)
            {
                string normalizedEntryName = entry.Name.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
                string filename = System.IO.Path.GetFileName(normalizedEntryName);
                string fileDestinationPath = destinationPath + normalizedEntryName;

                if (wildcardPattern.IsMatch(filename))
                {

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

                    }
                    else
                    {
                        //Check if parent directory exists
                        var parentDirectory = System.IO.Directory.GetParent(fileDestinationPath);
                        if (!parentDirectory.Exists) parentDirectory.Create();

                        //Create the file
                        entry.ExtractToFile(fileDestinationPath, overwrite);
                    }
                }

                entry = _tarReader.GetNextEntry();
            }
        }

        public bool HasOneTopLevelEntries()
        {
            var entryNames = new List<string>();
            var entry = _tarReader.GetNextEntry();
            while (entry != null)
            {
                entryNames.Add(entry.Name);
                entry = _tarReader.GetNextEntry();
            }

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
