using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    struct ProcessingPath
    {
        //The entry prefix
        public string BasePath { get; init; }

        public List<string> ChildPaths { get; init; }

        public ProcessingPath(string basePath, List<string> childPaths)
        {
            BasePath = basePath;
            ChildPaths = childPaths;
        }
    }
}
