using System.IO;
using System.Management.Automation;

namespace Microsoft.PowerShell.Archive
{
    [Cmdlet("Compress", "Archive", SupportsShouldProcess=true)]
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

        public SwitchParameter PassThru { get; set; } = false;


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

            if (ShouldProcess(DestinationPath, "Compress-Archive"))
            {
                //Create an archive
                ZipArchive zipArchive;
                if (Update)
                {
                    zipArchive = ZipArchive.OpenForUpdating(DestinationPath);
                }
                else
                {
                    zipArchive = ZipArchive.Create(DestinationPath);
                }
                zipArchive.SetCompressionLevel(CompressionLevel);

                //Process the entry records
                foreach (var entry in entryRecords)
                {
                    zipArchive.AddItem(entry);
                }

                //Dispose the archive
                zipArchive.Dispose();
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
                } else
                {
                    //Throw an error 
                    var errorMessage = String.Format(ErrorMessages.ZipFileExistError, path);
                    var exception = new System.InvalidOperationException(errorMessage);
                    ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveCmdletArchiveExists", System.Management.Automation.ErrorCategory.InvalidArgument, path);
                    ThrowTerminatingError(errorRecord);
                }
            }

            return path;
        }
    }
}