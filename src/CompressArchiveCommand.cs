using System.IO;
using System.Management.Automation;

namespace Microsoft.PowerShell.Archive
{
    [Cmdlet("Compress", "Archive", SupportsShouldProcess=true)]
    [OutputType(typeof(System.IO.FileInfo))]
    public class CompressArchiveCommand : Microsoft.PowerShell.Commands.CoreCommandBase
    {


        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "PathWithForce", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "PathWithUpdate", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[]? Path { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "LiteralPath", ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPathWithForce", ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPathWithUpdate", ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [Alias("PSPath")]
        public string[]? LiteralPath { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string? DestinationPath { get; set; }


        [Parameter(Mandatory = false, ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        [ValidateSet("Optimal", "NoCompression", "Fastest")]
        public string? CompressionLevel { get; set; }

        [Parameter(Mandatory = true, ParameterSetName="PathWithUpdate", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false)]
        [Parameter(Mandatory = true, ParameterSetName="LiteralPathWithUpdate", ValueFromPipeline=false, ValueFromPipelineByPropertyName=false)]
        public SwitchParameter Update { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "PathWithForce", ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPathWithForce", ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        public override SwitchParameter Force { get { return base.Force; } set { base.Force = value; } }

        [Parameter()]
        public SwitchParameter PassThru { get; set; } = false;

        public string? Format { get; set; }


        //Store source paths as they were inputted
        private List<string> _inputPaths;

        public CompressArchiveCommand()
        {
            _inputPaths = new List<string>();
        }


        protected override void BeginProcessing()
        {
            //Step 1: get destination path
            DestinationPath = GetDestinationPath();

            
            

            base.BeginProcessing();
        }

       
        protected override void ProcessRecord()
        {
            //Step 2: Add path/literal path to input paths
            if (ParameterSetName.StartsWith("Path")) _inputPaths.AddRange(Path);
            else _inputPaths.AddRange(LiteralPath);


            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            //Resolve the source paths
            PathHelper pathHelper = new PathHelper();
            pathHelper.Cmdlet = this;
            List<EntryRecord> entryRecords = pathHelper.GetNonLiteralPath(Path);

            //Find duplicates from input paths
            var duplicates = entryRecords.GroupBy(x => x.Name)
                                        .Where(group => group.Count() > 1)
                                        .Select(x => x.Key);

            //Report an error if there are duplicates
            //If there a positive number of duplicates, throw an error
            if (duplicates.Count() > 0)
            {
                var parameterName = ParameterSetName.StartsWith("Path") ? "Path" : "LiteralPath";
                var commaSeperatedDuplicates = String.Join(",", duplicates);
                var errorMessage = String.Format(ErrorMessages.DuplicatePathFoundError, parameterName, commaSeperatedDuplicates);
                var exception = new System.InvalidOperationException(errorMessage);
                ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveCmdletDuplicatePaths", System.Management.Automation.ErrorCategory.InvalidArgument, duplicates);
                ThrowTerminatingError(errorRecord);
            }

            if (entryRecords.Count == 0)
            {
                WriteVerbose("Creating an archive containing 0 entries");
            }

            Format = "Tar";

            if (ShouldProcess(DestinationPath, "Compress-Archive"))
            {
                //Create an archive
                ZipArchive zipArchive = null;
                TarArchive tarArchive = null;
                if (Update)
                {
                    zipArchive = ZipArchive.OpenForUpdating(DestinationPath);
                }
                else
                {
                    if (Format == "Tar")
                    {
                        tarArchive = TarArchive.Create(DestinationPath);
                    } else
                    {
                        zipArchive = ZipArchive.Create(DestinationPath);
                    }
                }
                if (Format == "Zip") zipArchive.SetCompressionLevel(CompressionLevel);

                int archivedEntries = 0;
                //Process the entry records
                foreach (var entry in entryRecords)
                {
                    if (Format == "Zip") zipArchive.AddItem(entry);
                    if (Format == "Tar") tarArchive.AddItem(entry);
                    archivedEntries++;
                    WriteVerbose($"Archived {entry.FullPath} ({archivedEntries}/{entryRecords.Count})");
                    float percentComplete = archivedEntries / entryRecords.Count * 100;
                    ProgressRecord progressRecord = new ProgressRecord(1, "Archiving in progress", $"{percentComplete}%");
                    WriteProgress(progressRecord);
                }

                //Dispose the archive
                if (Format == "Zip") zipArchive.Dispose();
                if (Format == "Tar") tarArchive.Dispose();
            }


            if (PassThru)
            {
                //Return a file representing the archive
                System.IO.FileInfo archiveFile = new FileInfo(DestinationPath);
                WriteObject(archiveFile);
            }


            base.EndProcessing();
        }

        //Get the full destination path given DestinationPath parameter
        private string GetDestinationPath()
        {
            //Get the unresolved path
            string path = GetUnresolvedProviderPathFromPSPath(DestinationPath);

            //Check if it is a folder
            if (System.IO.Directory.Exists(path))
            {
                //if so, append folder's file name to it
                var directoryInfo = new DirectoryInfo(path);
                path += directoryInfo.Name + ".zip";
            }

            //Check if the path points to an existing file
            if (System.IO.File.Exists(path))
            {
                if (Force)
                {
                    //Remove the file
                    System.IO.File.Delete(path);
                    WriteVerbose("Archive file already exists, deleting it");
                } else if (Update)
                {
                    //Check file permissions 
                    //Throw an error if the file is read only
                }
                else
                {
                    //Throw an error 
                    var errorMessage = String.Format(ErrorMessages.ZipFileExistError, path);
                    var exception = new System.InvalidOperationException(errorMessage);
                    ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveCmdletArchiveExists", System.Management.Automation.ErrorCategory.InvalidArgument, path);
                    ThrowTerminatingError(errorRecord);
                }
            } else if (!System.IO.File.Exists(path) && Update)
            {
                //Throw an error
            }

            return path;
        }
    }
}