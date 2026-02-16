using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.TimelineExport;

public interface ITimelineExport
{
    Task ExportAsync(TimelineExportOptions options, CancellationToken ct = default);
}
