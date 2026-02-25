using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.FileExportOrchestrator;

public sealed class FileExportOrchestrator(
    ILogger<FileExportOrchestrator> _logger,
    IFileStorage _fileStorage,
    IGooglePhotosRepository _googlePhotosRepository,
    ISteamRepository _steamRepository) : IFileExportOrchestrator
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

    public Task<FileExportResult> ExportChromecastPhotosAsync(FileExportOptions options, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileExportResult> ExportDesktopWallpapers4kAsync(FileExportOptions options, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileExportResult> ExportDesktopWallpapersWQHDAsync(FileExportOptions options, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileExportResult> ExportSteamScreenshotsAsync(FileExportOptions options, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<FileExportResult> ExportJottacloudTimelineAsync(FileExportOptions options, CancellationToken ct)
    {
        if (!_fileStorage.ValidateDirectory(new DirectoryOptions(options.SourceRoot, false)))
        {
            _logger.LogError(
                "Source folder with name @{FolderName} does not exist",
                options.SourceRoot);

            return new FileExportResult(false);
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(options.DestinationRoot, true)))
        {
            _logger.LogError(
                "Could not create destination folder with name @{FolderName}",
                options.DestinationRoot);

            return new FileExportResult(false);
        }

        foreach (var file in _fileStorage.EnumerateFiles(options.SourceRoot))
        {
            var timestamp = _fileStorage.GetFileTimestampFromLastWriteTime(file);
            var structuredDestinationDirectory = GetTargetDirectoryNameFromFileTimestamp(options.DestinationRoot, file, timestamp);
            var copyResult = await _fileStorage.CopyAsync(file, structuredDestinationDirectory, false, ct);

            if (!copyResult.Success)
            {
                _logger.LogError("Failed to copy file: {FilePath}", file);
                return new FileExportResult(false);
            }

            if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
            {
                _logger.LogError(
                    "File was copied, but file content does not match: {FilePath}",
                    copyResult.targetFileFullPath);
                return new FileExportResult(false);
            }

            if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
            {
                _logger.LogError(
                    "File was copied, but metadata does not match: {FilePath}",
                    copyResult.targetFileFullPath);
                return new FileExportResult(false);
            }

            _logger.LogInformation("Copied file: {FilePath}", copyResult.targetFileFullPath);
        }

        return new FileExportResult(true);
    }

    public string GetTargetDirectoryNameFromFileTimestamp(string destinationRootPath, string fileFullPath, DateTime fileCreationTime)
    {
        string year = fileCreationTime.Year.ToString();
        int monthIndex = fileCreationTime.Month-1;
        string monthDirectoryName = MonthDirectoryNames[monthIndex];

        return Path.Combine(destinationRootPath, year, monthDirectoryName);
    }
}
