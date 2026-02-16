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
        if (!_fileStorage.ValidateFolder(new FolderOptions(options.SourceRoot, false)))
        {
            _logger.LogError(
                "Source folder with name @{FolderName} does not exist",
                options.SourceRoot);

            return;
        }
        else if (!_fileStorage.ValidateFolder(new FolderOptions(options.DestinationRoot, true)))
        {
            _logger.LogError(
                "Could not create destination folder with name @{FolderName}",
                options.DestinationRoot);

            return;
        }

        foreach (var file in _fileStorage.EnumerateFiles(options.SourceRoot))
        {
            var timestamp = _fileStorage.GetFileTimestamp(file);
            var folderName = GetFolderName(options.DestinationRoot, timestamp);
            await _fileStorage.CopyAsync(file, folderName, ct);
        }
        
        await Task.FromResult(true);
    }

    public string GetFolderName(string destinationRootPath, DateTime fileCreationTime)
    {
        var year = fileCreationTime.Year.ToString();
        var month = fileCreationTime.Month;

        var monthName = ""; // TODO: Get;

        var folderName = $"{month}-{monthName}";

        return Path.Combine(destinationRootPath, year, folderName);
    }
}
