using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    internal class PathHelper
    {
        public PSCmdlet Cmdlet { get; set; }

        public List<EntryRecord> GetNonLiteralPath(string[] paths, string entryPrefix = null)
        {

            List<EntryRecord> records = new List<EntryRecord>();

            //Step 1: Go through each path

            foreach (var path in paths)
            {
                //Step 2: Resolve the path
                foreach (var resolvedPath in Cmdlet.GetResolvedProviderPathFromPSPath(path, out var providerPath))
                {
                    //Omitted: If the path is not from the filesystem, throw an error

                    //Set entry record
                    EntryRecord record = new EntryRecord();
                    string recordEntryPrefix = entryPrefix ?? System.IO.Path.GetDirectoryName(resolvedPath) + System.IO.Path.DirectorySeparatorChar;
                    

                    string fullPath = resolvedPath;
                    if (System.IO.Directory.Exists(resolvedPath))
                    {
                        if (!resolvedPath.EndsWith(System.IO.Path.DirectorySeparatorChar)) fullPath += System.IO.Path.DirectorySeparatorChar;

                        foreach (string child in System.IO.Directory.EnumerateFileSystemEntries(resolvedPath, "*"))
                        {
                            records.AddRange(GetNonLiteralPath(new string[] { child }, recordEntryPrefix));
                        }
                        
                    }

                    record.FullPath = resolvedPath;
                    record.Name = fullPath.Replace(recordEntryPrefix, "");
                    records.Add(record);
                }
            }

            return records;
        }
    }
}
