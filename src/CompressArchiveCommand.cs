using System.IO;
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
            //Step 3: Find duplicates from input paths
            _inputPaths.ForEach(x => x = x.Trim());
            var duplicates = _inputPaths.GroupBy(x => x)
                                        .Where(group => group.Count() > 1)
                                        .Select(x => x.Key);

            //Step 4: Report an error if there are duplicates
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

            //Step 5: Resolve Path
            List<ProcessingPath> processingPaths;
            if (ParameterSetName.StartsWith("Path")) processingPaths = ResolveNonLiteralPaths();
            else processingPaths = ResolveLiteralPaths();

            //Step 6: Create the archive

            //Step 7: Add each file to it

            //Compress each file
            //CreateZipArchive();


            base.EndProcessing();
        }

        private List<ProcessingPath> ResolveLiteralPaths()
        {
            List<ProcessingPath> processingPaths = new List<ProcessingPath>();
            List<string> nonexistantPaths = new List<string>();
            foreach (var path in _inputPaths)
            {
                //Get the unresolved path
                string unresolvedPath = GetUnresolvedProviderPathFromPSPath(path);

                //Get # of ancestor directories to keep
                int numberOfAncestorDirectoriesToKeep = path.Count(x => x == System.IO.Path.DirectorySeparatorChar || x == System.IO.Path.AltDirectorySeparatorChar);
                if (path.EndsWith(System.IO.Path.DirectorySeparatorChar) && path.Length > 1)
                {
                    numberOfAncestorDirectoriesToKeep--;
                }

                //Check if the path exists
                if (System.IO.Directory.Exists(unresolvedPath))
                {
                    // Make sure the path has a '/' at the end
                    if (!unresolvedPath.EndsWith(System.IO.Path.PathSeparator)) unresolvedPath += System.IO.Path.DirectorySeparatorChar;

                    //Add '*' at the end and call ResolveNonLiteralPaths if the directory is not empty
                    if (System.IO.Directory.EnumerateFileSystemEntries(unresolvedPath).Count() > 0)
                    {
                        //Add '*' to end
                        unresolvedPath += "*";

                        //Call ResolveNonLiteralPaths
                        List<ProcessingPath> directoryProcessingPaths = ResolveNonLiteralPaths();
                        directoryProcessingPaths.ForEach(x => x = new ProcessingPath(x.FullPath, numberOfAncestorDirectoriesToKeep + x.NumberOfAncestorDirectoriesToKeep));
                        processingPaths.AddRange(directoryProcessingPaths);
                        continue;
                    }
                } 
                else if (!System.IO.File.Exists(unresolvedPath))
                {
                    //Add path to nonexistantPaths
                    nonexistantPaths.Add(path);
                    continue;
                }

                
                WriteObject($"For path {path}: {numberOfAncestorDirectoriesToKeep}");

                //Finally, create a struct with path info
                ProcessingPath processingPath = new ProcessingPath(unresolvedPath, numberOfAncestorDirectoriesToKeep);
                processingPaths.Add(processingPath);
            }

            //Throw an error if we have nonexistant paths
            if (nonexistantPaths.Count > 0)
            {
                var commaSeperatedPaths = String.Join(',', nonexistantPaths);
                var errorMessage = String.Format(ErrorMessages.PathNotFoundError, commaSeperatedPaths);
                var exception = new System.InvalidOperationException(errorMessage);
                ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveCmdletInvalidPath", System.Management.Automation.ErrorCategory.InvalidArgument, nonexistantPaths);
                ThrowTerminatingError(errorRecord);

            }

            return processingPaths;
        }

        //Need to add a parameter for a collection
        private List<ProcessingPath> ResolveNonLiteralPaths()
        {
            List<ProcessingPath> processingPaths = new List<ProcessingPath>();
            List<string> nonexistantPaths = new List<string>();
            foreach (var path in _inputPaths)
            {
                //Get the unresolved path
                ProviderInfo info;
                var resolvedPaths = GetResolvedProviderPathFromPSPath(path, out info);

                //Check if the path belongs to the filesystem, otherwise add it to nonexistant paths
                if (info.Name != "FileSystem")
                {
                    nonexistantPaths.Add(path);
                    continue;
                }

                //Get # of ancestor directories to keep
                int numberOfAncestorDirectoriesToKeep = path.Count(x => x == System.IO.Path.DirectorySeparatorChar || x == System.IO.Path.AltDirectorySeparatorChar);
                if (path.EndsWith(System.IO.Path.DirectorySeparatorChar) && path.Length > 1)
                {
                    numberOfAncestorDirectoriesToKeep--;
                }

                foreach (var resolvedPath in resolvedPaths)
                {
                    string finalResolvedPath = resolvedPath;

                    //Check if the path exists
                    if (System.IO.Directory.Exists(finalResolvedPath))
                    {
                        // Make sure the path has a '/' at the end
                        if (!finalResolvedPath.EndsWith(System.IO.Path.PathSeparator)) finalResolvedPath += System.IO.Path.DirectorySeparatorChar;
                    }
                    
                    WriteObject($"For path {resolvedPath}: {numberOfAncestorDirectoriesToKeep}");

                    //Finally, create a struct with path info
                    ProcessingPath processingPath = new ProcessingPath(finalResolvedPath, numberOfAncestorDirectoriesToKeep);
                    processingPaths.Add(processingPath);
                }
            }

            //Throw an error if we have nonexistant paths
            if (nonexistantPaths.Count > 0)
            {
                var commaSeperatedPaths = String.Join(',', nonexistantPaths);
                var errorMessage = String.Format(ErrorMessages.PathNotFoundError, commaSeperatedPaths);
                var exception = new System.InvalidOperationException(errorMessage);
                ErrorRecord errorRecord = new ErrorRecord(exception, "ArchiveCmdletInvalidPath", System.Management.Automation.ErrorCategory.InvalidArgument, nonexistantPaths);
                ThrowTerminatingError(errorRecord);

            }

            return processingPaths;
        }

        private void CreateZipArchive()
        {
            ZipArchive zipArchive = ZipArchive.Create(DestinationPath);

            foreach (var currentItem in _inputPaths)
            {
                zipArchive.AddItem(currentItem);
            }

            zipArchive.Dispose();
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