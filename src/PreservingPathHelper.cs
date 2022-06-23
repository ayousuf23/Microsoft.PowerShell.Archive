using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    internal class PreservingPathHelper
    {


        public List<EntryRecord> GetNonLiteralPath(string[] paths, string entryPrefix = null)
        {
            //Get entry record for non literal path while preserving relative directory structure

            List<EntryRecord> result = new List<EntryRecord>();
            PathHelper pathHelper = new PathHelper();
            pathHelper.Cmdlet = this;

            //If path is absolute, process it normally
            foreach (var path in paths)
            {
                if (IsAbsolutePath(path) || PathsContainsTilda(path))
                {
                    result.AddRange(pathHelper.GetNonLiteralPath(new string[] { path }));
                } else
                {
                    foreach (var resolvedPath in Cmdlet.GetResolvedProviderPathFromPSPath(path, out var providerPath))
                    {
                        //Omitted: If the path is not from the filesystem, throw an error

                        if (entryPrefix == null)
                        {
                            Cmdlet.WriteObject($"Parent: {preservingPathHelper.GetTopMostDirectory(path, resolvedPath)} Resolved path: {resolvedPath}");
                        }


                        //Set entry record
                        EntryRecord record = new EntryRecord();
                        string recordEntryPrefix = entryPrefix ?? System.IO.Path.GetDirectoryName(resolvedPath) + System.IO.Path.DirectorySeparatorChar;
                        string recordEntryPrefix = 


                        string fullPath = resolvedPath;
                        if (System.IO.Directory.Exists(resolvedPath))
                        {
                            if (!resolvedPath.EndsWith(System.IO.Path.DirectorySeparatorChar)) fullPath += System.IO.Path.DirectorySeparatorChar;

                            foreach (string child in System.IO.Directory.EnumerateFileSystemEntries(resolvedPath, "*"))
                            {
                                records.AddRange(GetNonLiteralPath(new string[] { child }, recordEntryPrefix));
                            }

                        }

                        result.FullPath = resolvedPath;
                        result.Name = fullPath.Replace(recordEntryPrefix, "");
                        result.Add(record);
                    }
                }
            }

            //else, do special processing
            return null;
        }

        private bool IsAbsolutePath(string path)
        {
            return System.IO.Path.IsPathRooted(path);
        }

        private bool PathsContainsTilda(string path)
        {
            return path.Contains('~');
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
