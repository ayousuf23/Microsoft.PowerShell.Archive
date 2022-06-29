using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    [Cmdlet("Expand", "Archive", SupportsShouldProcess = true)]
    [OutputType(typeof(System.IO.FileInfo))]
    public class ExpandArchiveCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "PathWithForce", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string? Path { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "LiteralPath", ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPathWithForce", ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [Alias("PSPath")]
        public string? LiteralPath { get; set; }

        [Parameter(Mandatory = false, Position = 1, ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string? DestinationPath { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "PathWithForce", ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPathWithForce", ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        public SwitchParameter Force { get; set; }

        [Parameter()]
        public SwitchParameter PassThru { get; set; } = false;

        [Parameter()]
        public ArchiveFormat? Format { get; set; } = null;

        [Parameter()]
        public string? Filter { get; set; } = "*";

        private List<string> _inputPaths;

        public ExpandArchiveCommand()
        {
            _inputPaths = new List<string>();
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName.StartsWith("Path")) _inputPaths.Add(Path);
            else _inputPaths.Add(LiteralPath);

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            if (_inputPaths.Count > 1)
            {
                //Throw a terminating error
                var errorMsg = "More than 1 path was entered";
                System.InvalidOperationException exception = new InvalidOperationException();
                ErrorRecord errorRecord = new ErrorRecord(exception, "MoreThanOnePath", ErrorCategory.InvalidArgument, _inputPaths);
                ThrowTerminatingError(errorRecord);
            }

            //Get the archive path
            string archivePath = GetArchivePath();

            //Based on the format, create the appropriate type of archive
            ZipArchive? zipArchive = null;
            TarArchive? tarArchive = null;
            TarGzArchive? tarGzArchive = null;

            if (Format == ArchiveFormat.zip) zipArchive = ZipArchive.OpenForReading(archivePath);

            //Get the destination path
            bool has1TLEntry = false;
            if (Format == ArchiveFormat.zip) has1TLEntry = zipArchive.HasOneTopLevelEntries();
            if (has1TLEntry && DestinationPath == null)
            {
                DestinationPath = SessionState.Path.CurrentLocation.Path;
                if (!DestinationPath.EndsWith(System.IO.Path.DirectorySeparatorChar)) DestinationPath += System.IO.Path.DirectorySeparatorChar;
            } else
            {
                DestinationPath = GetDestinationPath(DestinationPath, archivePath);
            }
            

            if (ShouldProcess(archivePath, "Expand"))
            {
                if (Format == ArchiveFormat.zip) zipArchive.ExpandArchive(DestinationPath, Force.IsPresent);

            }

            base.EndProcessing();
        }

        private string GetArchivePath()
        {
            //Get the unresolved path
            string path = PathHelper.ResolvePath(_inputPaths[0], this);

            //Check if it is a folder
            if (System.IO.Directory.Exists(path))
            {
                //Throw an error
                var errorMsg = "The source is a directory.";
                ThrowTerminatingErrorHelper("InvalidPath", new InvalidOperationException(errorMsg), ErrorCategory.InvalidArgument, path);
            } else if (!System.IO.File.Exists(path))
            {
                var errorMsg = "The source path does not exist.";
                ThrowTerminatingErrorHelper("InvalidPath", new InvalidOperationException(errorMsg), ErrorCategory.InvalidArgument, path);
            }

            //Check if it has an extension, and if not, add the appropriate extension
            string extension = System.IO.Path.GetExtension(path);

            //Check if the destination path has an extension and set the Format accordingly
            if (extension == ".zip" && Format == null)
            {
                Format = ArchiveFormat.zip;
                WriteVerbose("Setting format to zip");
            }
            else if (extension == ".tar" && Format == null)
            {
                Format = ArchiveFormat.tar;
                WriteVerbose("Setting format to tar");
            }
            else if (extension == ".gz" && Format == null)
            {
                Format = ArchiveFormat.targz;
                WriteVerbose("Setting format to tar.gz");
            }
            else if (Format == null)
            {
                Format = ArchiveFormat.zip;
                WriteVerbose("Format not specified, zip chosen by default");
            }

            return path;
        }

        private string GetDestinationPath(string path, string archivePath)
        {
            if (path == null)
            {
                var filename = System.IO.Path.GetFileNameWithoutExtension(archivePath);
                path = System.IO.Path.Join(SessionState.Path.CurrentLocation.Path, filename);
            }

            string resolvedPath = PathHelper.ResolvePath(path, this);

            //Check if it is a folder
            if (System.IO.File.Exists(resolvedPath))
            {
                //Throw an error
                var errorMsg = "The destination cannot be a file.";
                ThrowTerminatingErrorHelper("InvalidPath", new InvalidOperationException(errorMsg), ErrorCategory.InvalidArgument, resolvedPath);
            }

            if (!resolvedPath.EndsWith(System.IO.Path.DirectorySeparatorChar)) resolvedPath += System.IO.Path.DirectorySeparatorChar;

            return resolvedPath;
        }

        private void ThrowTerminatingErrorHelper(string errorId, Exception exception, ErrorCategory errorCategory, object target)
        {
            ErrorRecord errorRecord = new ErrorRecord(exception, errorId, errorCategory, target);
            ThrowTerminatingError(errorRecord);
        }
        
    }
}
