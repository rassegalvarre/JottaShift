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
        if (!_fileStorage.ValidateDirectory(new DirectoryOptions(options.SourceRoot, false)))
        {
            _logger.LogError(
                "Source folder with name @{FolderName} does not exist",
                options.SourceRoot);

            return;
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(options.DestinationRoot, true)))
        {
            _logger.LogError(
                "Could not create destination folder with name @{FolderName}",
                options.DestinationRoot);

            return;
        }

        foreach (var directory in _fileStorage.EnumerateDirectories(options.SourceRoot))
        {
            foreach (var file in _fileStorage.EnumerateFiles(directory))
            {
                var timestamp = _fileStorage.GetFileTimestamp(file);
                var folderName = GetFullFileName(options.DestinationRoot, file, timestamp);
                await _fileStorage.CopyAsync(file, folderName, false, ct);
            }
        }
        
        await Task.FromResult(true);
    }

    public string GetFullFileName(string destinationRootPath, string fileName, DateTime fileCreationTime)
    {
        var year = fileCreationTime.Year.ToString();
        var month = fileCreationTime.Month;

        var monthName = ""; // TODO: Get;

        var folderName = $"{month}-{monthName}";

        return Path.Combine(destinationRootPath, year, folderName, fileName);
    }
}
