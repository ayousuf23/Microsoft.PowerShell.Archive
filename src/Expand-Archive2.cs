using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    [Cmdlet("Expand", "Archive2", SupportsShouldProcess = true)]
    [OutputType(typeof(System.IO.FileInfo))]
    public class Expand_Archive2 : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public object[]? Path { get; set; }

        List<System.IO.DirectoryInfo> directories = new List<DirectoryInfo>();

        List<System.IO.FileInfo> fileInfos = new List<System.IO.FileInfo>();

        PathTree pathTree = new PathTree();

        protected override void ProcessRecord()
        {
            foreach (var item in Path)
            {
                if (item is FileInfo)
                {
                    fileInfos.Add((FileInfo)item);
                    WriteObject("File");
                } else if (item is DirectoryInfo)
                {
                    directories.Add((DirectoryInfo)item);
                    WriteObject("Directory");
                }
            }

            //WriteWarning("hello");
            //ErrorRecord errorRecord = new ErrorRecord(new Exception(), "error", ErrorCategory.InvalidOperation, null);
            //WriteError(errorRecord);

            base.ProcessRecord();
        }

        protected override void EndProcessing()
        {
            //Add directories to trees
            foreach (var folder in directories)
            {
                pathTree.AddPath(folder.FullName);
            }

            //Add files to tree
            foreach (var file in fileInfos)
            {
                pathTree.AddPath(file.FullName);
            }

            //Get file paths
            foreach (var file in fileInfos)
            {
                var path = pathTree.GetAvailablePath(file.FullName);
                WriteObject(path);
            }


            base.EndProcessing();
        }
    }
}
