using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.TimelineExport;

public sealed class TimelineExportService(
    ILogger<TimelineExportService> _logger,
    IFileStorage _fileStorage) : ITimelineExport
{
    // TODO: Replace hardcoded list
    private readonly string[] MonthDirectoryNames =
    [
        "01 Januar",
        "02 Februar",
        "03 Mars",
        "04 April",
        "05 Mai",
        "06 Juni",
        "07 Juli",
        "08 August",
        "09 September",
        "10 Oktober",
        "11 November",
        "12 Desember"
    ];

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
            _logger.LogInformation("Copying files from directory: {Directory}" , directory);

            foreach (var file in _fileStorage.EnumerateFiles(directory))
            {
                var timestamp = _fileStorage.GetFileTimestamp(file);
                var fileFullPath = GetFullFileName(options.DestinationRoot, file, timestamp);
                await _fileStorage.CopyAsync(file, fileFullPath, false, ct);

                _logger.LogInformation("Copied file: {FilePath}", fileFullPath);
            }
        }
        
        await Task.FromResult(true);
    }

    public string GetFullFileName(string destinationRootPath, string fileName, DateTime fileCreationTime)
    {
        var year = fileCreationTime.Year.ToString();
        var monthIndex = fileCreationTime.Month-1;
        var monthDirectoryName = MonthDirectoryNames[monthIndex];

        return Path.Combine(destinationRootPath, year, monthDirectoryName, fileName);
    }
}
