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

        foreach (var file in _fileStorage.EnumerateFiles(options.SourceRoot))
        {
            var timestamp = _fileStorage.GetFileTimestamp(file);
            var structuredDestinationDirectory = GetTargetDirectoryNameFromFileTimestamp(options.DestinationRoot, file, timestamp);
            var copyResult = await _fileStorage.CopyAsync(file, structuredDestinationDirectory, false, ct);

            if (!copyResult.Success)
            {
                _logger.LogError("Failed to copy file: {FilePath}", file);
                break;
            }

            if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
            {
                _logger.LogError(
                    "File was copied, but file content does not match: {FilePath}",
                    copyResult.targetFileFullPath);
                break;
            }

            if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
            {
                _logger.LogError(
                    "File was copied, but metadata does not match: {FilePath}",
                    copyResult.targetFileFullPath);
                break;
            }

            _logger.LogInformation("Copied file: {FilePath}", copyResult.targetFileFullPath);
        }

        await Task.FromResult(true);
    }

    public string GetTargetDirectoryNameFromFileTimestamp(string destinationRootPath, string fileFullPath, DateTime fileCreationTime)
    {
        string year = fileCreationTime.Year.ToString();
        int monthIndex = fileCreationTime.Month-1;
        string monthDirectoryName = MonthDirectoryNames[monthIndex];

        return Path.Combine(destinationRootPath, year, monthDirectoryName);
    }
}
