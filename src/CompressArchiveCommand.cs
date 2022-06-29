using System.IO;
using System.Management.Automation;

namespace Microsoft.PowerShell.Archive
{
    [Cmdlet("Compress", "Archive", SupportsShouldProcess = true)]
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
        [ValidateSet("Optimal", "NoCompression", "Fastest", "SmallestSize")]
        public string? CompressionLevel { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "PathWithUpdate", ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPathWithUpdate", ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        public SwitchParameter Update { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "PathWithForce", ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPathWithForce", ValueFromPipeline = false, ValueFromPipelineByPropertyName = false)]
        public override SwitchParameter Force { get { return base.Force; } set { base.Force = value; } }

        [Parameter()]
        public SwitchParameter PassThru { get; set; } = false;

        [Parameter()]
        public ArchiveFormat? Format { get; set; } = null;

        [Parameter()]
        public string? Filter { get; set; } = "*";

        [Parameter()]
        public SwitchParameter Flatten {get; set;}


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
            pathHelper.Filter = Filter;

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
                ErrorRecord errorRecord = new ErrorRecord(exception, "DuplicatePathFound", System.Management.Automation.ErrorCategory.InvalidArgument, duplicates);
                ThrowTerminatingError(errorRecord);
            }

            if (entryRecords.Count == 0)
            {
                WriteVerbose("Creating an archive containing 0 entries");
            }

            if (ShouldProcess(DestinationPath, "Compress-Archive"))
            {
                //Create an archive
                ZipArchive zipArchive = null;
                TarArchive tarArchive = null;
                TarGzArchive tarGzArchive = null;
                if (Update)
                {
                    if (Format == ArchiveFormat.zip) zipArchive = ZipArchive.OpenForUpdating(DestinationPath);
                    if (Format == ArchiveFormat.tar) tarArchive = TarArchive.OpenForUpdating(DestinationPath);
                    if (Format == ArchiveFormat.targz) tarGzArchive = TarGzArchive.OpenForUpdating(DestinationPath);
                }
                else
                {
                    if (Format == ArchiveFormat.tar) tarArchive = TarArchive.Create(DestinationPath);
                    else if (Format == ArchiveFormat.zip) zipArchive = ZipArchive.Create(DestinationPath);
                    else if (Format == ArchiveFormat.targz) tarGzArchive = TarGzArchive.Create(DestinationPath);
                }
                if (Format == ArchiveFormat.zip) zipArchive.SetCompressionLevel(CompressionLevel);

                ProgressRecord progressRecord = new ProgressRecord(1, "Archiving in progress", "0%");
                WriteProgress(progressRecord);

                int archivedEntries = 0;
                //Process the entry records
                foreach (var entry in entryRecords)
                {
                    if (Format == ArchiveFormat.zip) zipArchive.AddItem(entry);
                    if (Format == ArchiveFormat.tar) tarArchive.AddItem(entry);
                    if (Format == ArchiveFormat.targz) tarGzArchive.AddItem(entry);
                    archivedEntries++;
                    WriteVerbose($"Archived {entry.FullPath} ({archivedEntries}/{entryRecords.Count})");
                    float percentComplete = archivedEntries / entryRecords.Count * 100;
                    progressRecord = new ProgressRecord(1, "Archiving in progress", $"{percentComplete}%");
                    WriteProgress(progressRecord);
                }

                //Dispose the archive
                if (Format == ArchiveFormat.zip) zipArchive.Dispose();
                if (Format == ArchiveFormat.tar) tarArchive.Dispose();
                if (Format == ArchiveFormat.targz)
                {
                    tarGzArchive.Compress();
                    tarGzArchive.Dispose();
                }

                WriteVerbose($"Created Archive {DestinationPath}");


                if (PassThru)
                {
                    //Return a file representing the archive
                    System.IO.FileInfo archiveFile = new FileInfo(DestinationPath);
                    WriteObject(archiveFile);
                }

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
                if (Format == null)
                {
                    Format = ArchiveFormat.zip;
                    WriteVerbose("Format not specified, zip chosen by default");
                }
                if (Format == ArchiveFormat.zip) path += directoryInfo.Name + ".zip";
                else if (Format == ArchiveFormat.tar) path += directoryInfo.Name + ".tar";
                else if (Format == ArchiveFormat.targz) path += directoryInfo.Name + ".tar.gz";
                WriteVerbose("Destination path is a folder, setting archive name as directory name + file extension");
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
            } else if (Format == null)
            {
                Format = ArchiveFormat.zip;
                WriteVerbose("Format not specified, zip chosen by default");
            }

            if (!Update)
            {
                if (Format == ArchiveFormat.zip && extension != ".zip")
                {
                    path += ".zip";
                    WriteVerbose("Adding .zip extension to destination path");
                }
                if (Format == ArchiveFormat.tar && extension != ".tar")
                {
                    path += ".tar";
                    WriteVerbose("Adding .tar extension to destination path");
                }
                if (Format == ArchiveFormat.targz && extension != ".tar.gz")
                {
                    path += ".tar.gz";
                    WriteVerbose("Adding .tar.gz extension to destination path");
                }
            }

            //Check if the path points to an existing file
            if (System.IO.File.Exists(path))
            {
                if (Force)
                {
                    //Remove the file if not on what if mode
                    object whatIfValue;
                    MyInvocation.BoundParameters.TryGetValue("WhatIf", out whatIfValue);

                    if (whatIfValue == null || (whatIfValue is bool && ((bool)whatIfValue) == false))
                    {
                        System.IO.File.Delete(path);
                        WriteVerbose("Archive file already exists, deleting it");
                    }


                }
                else if (Update)
                {
                    //Check file permissions 
                    //Throw an error if the file is read only
                }
                else
                {
                    //Throw an error 
                    var errorMessage = String.Format(ErrorMessages.ZipFileExistError, path);
                    var exception = new System.InvalidOperationException(errorMessage);
                    ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveFileExists", System.Management.Automation.ErrorCategory.InvalidArgument, path);
                    ThrowTerminatingError(errorRecord);
                }

            }
            else if (!System.IO.File.Exists(path) && Update)
            {
                //Throw an error
                var errorMessage = "Archive file does not exist";
                var exception = new System.InvalidOperationException(errorMessage);
                ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveFileNotFound", System.Management.Automation.ErrorCategory.InvalidArgument, path);
                ThrowTerminatingError(errorRecord);
            }

            return path;
        }
    }
}