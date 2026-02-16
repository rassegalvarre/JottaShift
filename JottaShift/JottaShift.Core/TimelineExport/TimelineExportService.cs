using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.TimelineExport;

public sealed class TimelineExportService(
    ILogger<TimelineExportService> _logger,
    IFileStorage _fileStorage) : ITimelineExport
{

    public async Task ExportAsync(TimelineExportOptions options, CancellationToken ct)
    {
        _logger.LogInformation("Hello from TimelineExportService");
        await Task.FromResult(true);
    }
}
