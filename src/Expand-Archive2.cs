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
        public System.IO.FileSystemInfo[]? Path { get; set; }

        [Parameter]
        public string DestinationPath { get; set; }

        List<System.IO.DirectoryInfo> directories = new List<DirectoryInfo>();

        List<System.IO.FileInfo> fileInfos = new List<System.IO.FileInfo>();

        PathTree pathTree = new PathTree();

        Dictionary<string, Node2> pathToNode = new Dictionary<string, Node2>();

        List<FileSystemInfo> infos = new List<FileSystemInfo>();

        protected override void ProcessRecord()
        {
            infos.AddRange(Path);
        }

        protected override void EndProcessing()
        {
            // Sort infos
            infos.Sort((info1, info2) => info1.FullName.CompareTo(info2.FullName));

            // Resolved destination
            DestinationPath = GetUnresolvedProviderPathFromPSPath(DestinationPath);

            ZipArchive zipArchive = ZipArchive.Create(DestinationPath);

            // Go through each info
            foreach (var info in infos)
            {
                // Create a node for it
                Node2 node = new Node2();
                node.Info = info;

                // Get parent name
                int parentNameIndex = info.FullName.LastIndexOf(System.IO.Path.DirectorySeparatorChar, info.FullName.Length - 2);
                string parentName = info.FullName.Substring(0, parentNameIndex + 1);

                // See if its parent node is available
                if (pathToNode.TryGetValue(parentName, out var node1))
                {
                    node.Prefix = node1.Prefix;
                } else
                {
                    node.Prefix = parentName;
                }

                // Add to dict
                if (info is System.IO.DirectoryInfo)
                {
                    pathToNode.Add(info.FullName + System.IO.Path.DirectorySeparatorChar, node);
                } else
                {
                    pathToNode.Add(info.FullName, node);
                }
                

                // Print
                WriteObject($"{info.FullName}: {node.Prefix}");

                string name = info.FullName.Substring(node.Prefix.Length);
                if (info is System.IO.DirectoryInfo)
                {
                    name += System.IO.Path.DirectorySeparatorChar;
                }

                var entryRecord = new EntryRecord()
                {
                    Name = name,
                    FullPath = info.FullName
                };
                zipArchive.AddItem(entryRecord);
            }

            zipArchive.Dispose();
        }

        class Node2
        {
            public System.IO.FileSystemInfo Info { get; set; }

            public string Prefix { get; set; }
        }
    }
}
