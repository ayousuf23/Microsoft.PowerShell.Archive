using System.Management.Automation;

namespace Microsoft.PowerShell.Archive
{
    [Cmdlet("Compress", "Archive", SupportsShouldProcess=true)]
    public class CompressArchiveCommand : PSCmdlet
    {


        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Path", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "PathWithForce", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "PathWithUpdate", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string[]? Path { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "LiteralPath", ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPathWithForce", ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPathWithUpdate", ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [Alias("PSPath")]
        public string[]? LiteralPath { get; set; }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
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
        public SwitchParameter Force { get; set; }

        public SwitchParameter PassThru { get; set; } = false;


        private ISet<string> _inputPaths;

        public CompressArchiveCommand()
        {
            _inputPaths = new HashSet<string>();
        }


        protected override void BeginProcessing()
        {

            DestinationPath = GetUnresolvedProviderPathFromPSPath(DestinationPath); //Get full destination path, even if it does not exist and do not expand wildcard characters
            
            

            base.BeginProcessing();
        }

        //Use this to append current path to inputPaths
        protected override void ProcessRecord()
        {
            //Resolve the path
            bool useLiteralPath = ParameterSetName.StartsWith("LiteralPath");
            if (useLiteralPath) AddLiteralPathToInputPaths();
            else AddPathToInputPaths();

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            //Compress each file
            CreateZipArchive(null);


            base.EndProcessing();
        }

        private void CreateZipArchive(string? sourceDirFullPath)
        {
            ZipArchive zipArchive = ZipArchive.Create(sourceDirFullPath, DestinationPath);

            foreach (var currentItem in _inputPaths)
            {
                zipArchive.AddItem(currentItem);
            }

            zipArchive.Dispose();
        }

        private void AddPathToInputPaths()
        {
            foreach (var path in Path)
            {
                //We want to resolve using ResolvedPath
                ProviderInfo providerInfo;
                var resolvedPaths = GetResolvedProviderPathFromPSPath(path, out providerInfo);
                if (providerInfo.Name != "FileSystem")
                {
                    var errorMessage = $"Invalid Path {path}";
                    var exception = new System.InvalidOperationException(errorMessage);
                    ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveCmdletPathNotFound", System.Management.Automation.ErrorCategory.InvalidArgument, path);
                    ThrowTerminatingError(errorRecord);
                }

                foreach (var resolvedPath in resolvedPaths)
                {
                    //Check for duplicates
                    if (!_inputPaths.Add(resolvedPath))
                    {
                        var errorMessage = $"Duplicate Path {resolvedPath}";
                        var exception = new System.InvalidOperationException(errorMessage);
                        ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveCmdletPathDuplicate", System.Management.Automation.ErrorCategory.InvalidArgument, resolvedPath);
                        ThrowTerminatingError(errorRecord);
                    }
                }
            }
        }

        private void AddLiteralPathToInputPaths()
        {
            foreach (var path in LiteralPath)
            {
                //Resolve path using unresolved path
                var unresolvedpath = GetUnresolvedProviderPathFromPSPath(path);
                if (!System.IO.File.Exists(unresolvedpath) && !System.IO.Directory.Exists(unresolvedpath))
                {
                    var errorMessage = $"Invalid or non-existant Path {path}";
                    var exception = new System.InvalidOperationException(errorMessage);
                    ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveCmdletPathNotFound", System.Management.Automation.ErrorCategory.InvalidArgument, path);
                    ThrowTerminatingError(errorRecord);
                }

                //Check for duplicates
                if (!_inputPaths.Add(unresolvedpath))
                {
                    var errorMessage = $"Duplicate Path {path}";
                    var exception = new System.InvalidOperationException(errorMessage);
                    ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveCmdletPathDuplicate", System.Management.Automation.ErrorCategory.InvalidArgument, path);
                    ThrowTerminatingError(errorRecord);
                }
            }
        }
    }
}