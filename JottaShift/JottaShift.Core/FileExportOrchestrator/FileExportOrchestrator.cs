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

    public Task<GooglePhotosUploadJobResult> ExportChromecastPhotosAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileTransferJobResult> ExportDesktopWallpapers4kAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileTransferJobResult> ExportDesktopWallpapersWQHDAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileTransferJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<FileTransferJobResult> ExportJottacloudTimelineAsync(CancellationToken ct)
    {
        const string jobKey = "jottacloud_timeline";

        var job = GetFileTransferJob(jobKey);
        if (job == null)
        {
            _logger.LogError("No file transfer job setting found with key: {JobKey}", jobKey);
            return FileTransferJobResult.Invalid(jobKey, "Missing job setting");
        }

        if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} does not exist",
                job.SourceDirectoryPath);

            return FileTransferJobResult.Invalid(jobKey, "Missing source directory");
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.TargetDirectoryPath, true)))
        {
            _logger.LogError(
                "Could not create target directory with name @{DirectoryName}",
                job.TargetDirectoryPath);

            return FileTransferJobResult.Invalid(jobKey, "Missing target directory");
        }

        var result = FileTransferJobResult.StartJob(job);

        foreach (var file in _fileStorage.EnumerateFiles(job.SourceDirectoryPath))
        {
            result.PrepareOperation(file);
            var timestamp = _fileStorage.GetImageDate(file);
            var structuredDestinationDirectory = GetTargetDirectoryNameFromFileTimestamp(job.TargetDirectoryPath, file, timestamp);

            result.StartOperation();
            
            var copyResult = await _fileStorage.CopyAsync(file, structuredDestinationDirectory, false, ct);

            if (!copyResult.Success)
            {
                return result.FailOperation($"File transfer failed");
            }

            if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
            {
                _logger.LogError(
                    "File was copied, but file content does not match: {FilePath}",
                    copyResult.targetFileFullPath);
                return result.FailOperation($"Mismatched file content");
            }

            if (!_fileStorage.DoesFileMetadataMatch(file, copyResult.targetFileFullPath))
            {
                _logger.LogError(
                    "File was copied, but metadata does not match: {FilePath}",
                    copyResult.targetFileFullPath);
                return result.FailOperation($"Mismatched file metadata");
            }

            result.CompleteOperation(copyResult.targetFileFullPath);
            _logger.LogInformation("Copied file: {FilePath}", copyResult.targetFileFullPath);
        }

        return result.CompleteJob();
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
