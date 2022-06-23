using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    internal class PreservingPathHelper
    {


        public List<EntryRecord> GetNonLiteralPath(string[] paths, string entryPrefix = null)
        {
            //Get entry record for non literal path while preserving relative directory structure

            //If path is absolute, process it normally
            foreach (var path in paths)
            {
                if (IsAbsolutePath(path) || PathsContainsTilda(path))
                {

                } else
                {

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
