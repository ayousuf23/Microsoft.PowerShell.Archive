using Microsoft.PowerShell.Commands;
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

        public bool Flatten { get; set; }

        public string? Filter { get; set; } = "*";

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
                    string recordEntryPrefix = entryPrefix ?? GetPathPrefix(path, resolvedPath);
                    

                    string fullPath = resolvedPath;
                    if (System.IO.Directory.Exists(resolvedPath))
                    {
                        if (!resolvedPath.EndsWith(System.IO.Path.DirectorySeparatorChar)) fullPath += System.IO.Path.DirectorySeparatorChar;

                        if (entryPrefix == null)
                        {
                            foreach (string child in System.IO.Directory.EnumerateFileSystemEntries(resolvedPath, Filter, SearchOption.AllDirectories))
                            {
                                records.AddRange(GetNonLiteralPath(new string[] { child }, recordEntryPrefix));
                            }
                        }
                        
                        
                    } else if (entryPrefix == null)
                    {
                        //Check if file matches wildcard pattern
                        WildcardPattern wildcardPattern = new WildcardPattern(Filter);
                        if (!wildcardPattern.IsMatch(System.IO.Path.GetFileName(resolvedPath))) continue;
                    }

                    record.FullPath = resolvedPath;
                    record.Name = fullPath.Replace(recordEntryPrefix, "");
                    records.Add(record);
                }
            }

            return records;
        }

        public List<EntryRecord> GetLiteralPath(string[] paths, string entryPrefix = null)
        {
            List<EntryRecord> records = new List<EntryRecord>();

            List<string> nonexistantPaths = new List<string>();

            //Step 1: Go through each path

            foreach (var path in paths)
            {
                //Resolve the path
                string resolvedPath = Cmdlet.GetUnresolvedProviderPathFromPSPath(path);

                //Ensure resolved path exists
                if (!System.IO.Path.Exists(resolvedPath))
                {
                    nonexistantPaths.Add(resolvedPath);
                    continue;
                }

                EntryRecord record = new EntryRecord();
                string recordEntryPrefix = entryPrefix ?? GetPathPrefix(path, resolvedPath);

                string fullPath = resolvedPath;
                if (System.IO.Directory.Exists(resolvedPath))
                {
                    if (!resolvedPath.EndsWith(System.IO.Path.DirectorySeparatorChar)) fullPath += System.IO.Path.DirectorySeparatorChar;

                    foreach (string child in System.IO.Directory.EnumerateFileSystemEntries(resolvedPath, "*"))
                    {
                        records.AddRange(GetLiteralPath(new string[] { child }, recordEntryPrefix));
                    }

                }

                record.FullPath = resolvedPath;
                record.Name = fullPath.Replace(recordEntryPrefix, "");
                records.Add(record);
            }

            if (nonexistantPaths.Count > 0)
            {
                var commaSeperatedNonexistantPaths = String.Join(",", nonexistantPaths);
                var errorMessage = $"The path(s) {commaSeperatedNonexistantPaths} are nonexistant";
                var exception = new System.InvalidOperationException(errorMessage);
                ErrorRecord errorRecord = new ErrorRecord(exception, "InvalidPath", System.Management.Automation.ErrorCategory.InvalidArgument, nonexistantPaths);
                Cmdlet.ThrowTerminatingError(errorRecord);
            }

            return records;
        }

        public string GetPathPrefix(string path, string fullPath)
        {

            if (Flatten || System.IO.Path.IsPathRooted(path) || path.Contains('~') || path.Contains(".."))
            {
                return System.IO.Path.GetDirectoryName(fullPath) + System.IO.Path.DirectorySeparatorChar;
            } else
            {
                //This is a relative path not containing ~ or ..
                return Cmdlet.SessionState.Path.CurrentFileSystemLocation.ProviderPath + System.IO.Path.DirectorySeparatorChar;
            }
        }

        public static string ResolvePath(string path, PSCmdlet cmdlet)
        {
            string unresolvedPath = cmdlet.GetUnresolvedProviderPathFromPSPath(path);

            return unresolvedPath;

        }
    }
}
