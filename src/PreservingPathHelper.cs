using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    internal class PreservingPathHelper
    {
        public PSCmdlet? Cmdlet { get; set; }

        PathHelper pathHelper;
        

        public List<EntryRecord> GetNonLiteralPath(string[] paths, string entryPrefix = null)
        {
            //Get entry record for non literal path while preserving relative directory structure

            List<EntryRecord> result = new List<EntryRecord>();

            pathHelper = new PathHelper();
            pathHelper.Cmdlet = Cmdlet;

            //If path is absolute, process it normally
            foreach (var path in paths)
            {
                if (IsAbsolutePath(path) || PathsContainsTilda(path) || path.Contains(".."))
                {
                    result.AddRange(pathHelper.GetNonLiteralPath(new string[] { path }));
                } else
                {
                    AddEntryRecordForNonLiteralPath(path, result);
                }
            }

            //else, do special processing
            return result;
        }

        private void AddEntryRecordForNonLiteralPath(string path, List<EntryRecord> records)
{
            foreach (var resolvedPath in Cmdlet.GetResolvedProviderPathFromPSPath(path, out var providerPath))
            {
                //Omitted: If the path is not from the filesystem, throw an error

                //Set entry record
                EntryRecord record = new EntryRecord();


                //Get the entry record prefix
                string recordEntryPrefix = GetPathPrefix(path);
                Cmdlet.WriteObject($"Path: {path}, prefix: {recordEntryPrefix}");


                string fullPath = resolvedPath;
                if (System.IO.Directory.Exists(resolvedPath))
                {
                    if (!resolvedPath.EndsWith(System.IO.Path.DirectorySeparatorChar)) fullPath += System.IO.Path.DirectorySeparatorChar;

                    foreach (string child in System.IO.Directory.EnumerateFileSystemEntries(resolvedPath, "*"))
                    {
                        records.AddRange(pathHelper.GetNonLiteralPath(new string[] { child }, recordEntryPrefix));
                    }

                }

                record.FullPath = resolvedPath;
                record.Name = fullPath.Replace(recordEntryPrefix, "");
                records.Add(record);
            }
        }

        private bool IsAbsolutePath(string path)
        {
            return System.IO.Path.IsPathRooted(path);
        }

        private bool PathsContainsTilda(string path)
        {
            return path.Contains('~');
        }

        public string GetPathPrefix(string path)
        {
            // Path is set to current working directory
            string prefixPath = Cmdlet.SessionState.Path.CurrentFileSystemLocation.ProviderPath + System.IO.Path.DirectorySeparatorChar;

            //Split the path by seperator
            //string[] pathcomponents = path.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

            return prefixPath;
        }


        //Used to get the entry prefix for a relative path
        public string GetTopMostDirectory(string path, string fullPath)
        {
            int hops = HopsToTopMostDirectory(path);

            var parent = System.IO.Directory.GetParent(fullPath);
            for (int i = 0; i < hops; i++)
            {
                parent = System.IO.Directory.GetParent(fullPath);
            }
            return parent.FullName;
        }

        //Get number of hops to top most directory
        private int HopsToTopMostDirectory(string path)
        {
            //Split path by directory seperator
            string[] components = path.Split(new char[] { '/', '\\' });

            int hops = 0;

            for (int i=components.Length-1; i>=0; i--)
            {
                //Skip the bottom most portion
                if (i == components.Length - 1) continue;

                string component = components[i];
                if (component == "..")
                {
                    hops--;
                } else if (component == ".")
                {
                    
                } else
                {
                    hops++;
                }
            }

            return hops;
        }
    }
}
