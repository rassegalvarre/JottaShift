using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.TimelineExport
{
    internal record TimelineExportOptions
    {
        public required string SourceRoot { get; init; }
        public required string DestinationRoot { get; init; }
    }
}
