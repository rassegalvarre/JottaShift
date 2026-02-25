using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.FileExportOrchestrator;

public sealed class FileExportOrchestrator(
    FileExportSettings _fileExportSettings,
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

    public Task<FileExportResult> ExportChromecastPhotosAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileExportResult> ExportDesktopWallpapers4kAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileExportResult> ExportDesktopWallpapersWQHDAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileExportResult> ExportSteamScreenshotsAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<FileExportResult> ExportJottacloudTimelineAsync(CancellationToken ct)
    {
        var job = GetFileTransferJob("jottacloud_timeline");
        if (job == null)
        {
            return new FileExportResult(false);
        }

        if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} does not exist",
                job.SourceDirectoryPath);

            return new FileExportResult(false);
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.TargetDirectoryPath, true)))
        {
            _logger.LogError(
                "Could not create target directory with name @{DirectoryName}",
                job.TargetDirectoryPath);

            return new FileExportResult(false);
        }

        foreach (var file in _fileStorage.EnumerateFiles(job.SourceDirectoryPath))
        {
            var timestamp = _fileStorage.GetFileTimestampFromLastWriteTime(file);
            var structuredDestinationDirectory = GetTargetDirectoryNameFromFileTimestamp(job.TargetDirectoryPath, file, timestamp);
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

    public FileTransferJob? GetFileTransferJob(string key)
    {
        return _fileExportSettings.FileTransferJobs.FirstOrDefault(j => j.Key == key);
    }

    public GooglePhotosUploadJob? GetGooglePhotosUploadJob(string key)
    {
        return _fileExportSettings.GooglePhotosUploadJobs.FirstOrDefault(j => j.Key == key);
    }
}
