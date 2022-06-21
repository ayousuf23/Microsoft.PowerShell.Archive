using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PowerShell.Archive
{
    internal class EntryRecord
    {
        public string? Name { get; set; }

        public string? FullPath { get; set; }
    }
}
