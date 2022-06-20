using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    struct ProcessingPath
    {
        public string FullPath { get; init; }

        public int NumberOfAncestorDirectoriesToKeep { get; init; }

        public ProcessingPath(string fullPath, int numberOfAncestorDirectoriesToKeep)
        {
            FullPath = fullPath;
            NumberOfAncestorDirectoriesToKeep = numberOfAncestorDirectoriesToKeep;
        }
    }
}
