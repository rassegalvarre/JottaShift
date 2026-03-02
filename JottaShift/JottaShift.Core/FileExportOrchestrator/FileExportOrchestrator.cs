using JottaShift.Core.FileExportOrchestrator.Jobs;
using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;
using JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace JottaShift.Core.FileExportOrchestrator;

public sealed class FileExportOrchestrator(
    ILogger<FileExportOrchestrator> _logger,
    IFileStorage _fileStorage,
    IFileExportJobValidator _fileExportJobValidator,
    IGooglePhotosRepository _googlePhotosRepository,
    ISteamRepository _steamRepository) : IFileExportOrchestrator
{
    private CultureInfo _culture { get; set; } = CultureInfo.CurrentCulture;

    public void SetCulture(CultureInfo culture)
    {
        _culture = culture;
    }

    // Todo: Handle (Conflict) files and dirs. Ignore?
    public async Task<GooglePhotosUploadJobResult> ExportChromecastPhotosAsync(CancellationToken ct = default)
    {
        const string jobKey = "chromecast_photos";
        var result = _fileExportJobValidator.ValidateGooglePhotosUploadJob(jobKey);
        if (result.PreValidationFailed && result.Job != null)
        {
            _logger.LogError("Job with key {JobKey} failed pre-validation and cannot be started", jobKey);
            return result;
        }

        result.StartJob();

        List<string> filePathsToUpload = [.. _fileStorage.EnumerateFiles(result.Job.SourceDirectoryPath)];
        if (filePathsToUpload.Count == 0)
        {
            _logger.LogInformation("No images staged for upload to Google Photos");
            return result.CompleteJob();
        }

        var filesUploaded = await _googlePhotosRepository.UploadImagesToAlbum(filePathsToUpload, result.Job.AlbumName);

        if (filesUploaded == 0)
        {
            return result.FailOperation($"No files were uploaded to Google");
        }

        if (filePathsToUpload.Count() != filesUploaded)
        {
            _logger.LogError(
                "Did not upload all images to Google: {FilesUploaded} out of {FileCount} were uploaded",
                filesUploaded, filePathsToUpload.Count());
            return result.FailOperation($"Missing files");
        }

        return result.CompleteJob();
    }

    public async Task<FileTransferJobResult> ExportDesktopWallpapersAsync(CancellationToken ct = default)
    {
        const string jobKey = "desktop_wallpapers";

        var result = _fileExportJobValidator.ValidateGooglePhotosUploadJob(jobKey);
        if (result.PreValidationFailed && result.Job != null)
        {
            _logger.LogError("Job with key {JobKey} failed pre-validation and cannot be started", jobKey);
            return result;
        }

        result.StartJob();

        foreach (var file in _fileStorage.EnumerateFiles(result.Job.SourceDirectoryPath))
        {
            var imageResolution = _fileStorage.GetImageResolution(file);
            string targetDirectoryForResolution;

            if (imageResolution.EndsWith("1440"))
            {
                targetDirectoryForResolution = "QHD";
            }
            else if (imageResolution.EndsWith("2160"))
            {
                targetDirectoryForResolution = "4K";
            }
            else if (imageResolution.EndsWith("1080"))
            {
                targetDirectoryForResolution = "FullHD";
            }
            else
            {
                _logger.LogWarning(
                    "Skipping file with name @{FileName} since it does not match expected format of ending with resolution",
                    file);

                continue;
            }

            result.PrepareOperation(file);

            string fullTargetDirectoryPath = Path.Combine(result.Job.TargetDirectoryPath, targetDirectoryForResolution);

            result.StartOperation();
            var copyResult = await _fileStorage.CopyAsync(file, fullTargetDirectoryPath, false, ct);
            if (!copyResult.Success)
            {
                return result.FailOperation($"File transfer failed for file {file}");
            }
            if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
            {
                _logger.LogError(
                    "File was copied, but file content does not match: {FilePath}",
                    copyResult.targetFileFullPath);
                return result.FailOperation($"Mismatched file content for file {file}");
            }
            if (!_fileStorage.DoesFileMetadataMatch(file, copyResult.targetFileFullPath))
            {
                _logger.LogError(
                    "File was copied, but metadata does not match: {FilePath}",
                    copyResult.targetFileFullPath);
                return result.FailOperation($"Mismatched file metadata for file {file}");
            }
            if (result.Job.DeleteSourceFiles && !_fileStorage.DeleteFile(file))
            {
                _logger.LogError(
                    "File was copied, but failed to delete source file: {FilePath}",
                    file);
            }
            result.CompleteOperation(copyResult.targetFileFullPath);
            _logger.LogInformation("Copied file: {FilePath}", copyResult.targetFileFullPath);
        }

        return result.CompleteJob();
    }

    public async Task<FileTransferJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default)
    {
        const string jobKey = "steam_screenshots";

        var result = _fileExportJobValidator.ValidateGooglePhotosUploadJob(jobKey);
        if (result.PreValidationFailed && result.Job != null)
        {
            _logger.LogError("Job with key {JobKey} failed pre-validation and cannot be started", jobKey);
            return result;
        }

        result.StartJob();
        foreach (var directory in _fileStorage.EnumerateDirectories(result.Job.SourceDirectoryPath))
        {
            var directoryNameToCharArray = Path.GetFileName(directory)?.ToCharArray();
            if (!uint.TryParse(directoryNameToCharArray, out uint appId))
            {
                _logger.LogWarning(
                    "Skipping directory with name @{DirectoryName} since it does not match expected format of a Steam appId",
                    directory);
                continue;
            }

            var appName = await _steamRepository.GetAppNameFromId(appId);

            if (string.IsNullOrEmpty(appName))
            {
                _logger.LogWarning("Could not find Steam app name for appId {AppId}", appId);
                continue;
            }

            string targetDirectoryForApp = Path.Combine(result.Job.TargetDirectoryPath, appName);

            if (!_fileStorage.ValidateDirectory(new DirectoryOptions(targetDirectoryForApp, true)))
            {
                _logger.LogError(
                    "Could not create target directory with name @{DirectoryName}",
                    targetDirectoryForApp);

                return result.FailOperation("Cannot create target directory");
            }

            foreach (var file in _fileStorage.EnumerateFiles(directory))
            {
                result.PrepareOperation(file);
                var copyResult = await _fileStorage.CopyAsync(file, targetDirectoryForApp, false, ct);
                if (!copyResult.Success)
                {
                    return result.FailOperation($"File transfer failed for file {file}");
                }
                if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
                {
                    _logger.LogError(
                        "File was copied, but file content does not match: {FilePath}",
                        copyResult.targetFileFullPath);
                    return result.FailOperation($"Mismatched file content for file {file}");
                }
                if (!_fileStorage.DoesFileMetadataMatch(file, copyResult.targetFileFullPath))
                {
                    _logger.LogError(
                        "File was copied, but metadata does not match: {FilePath}",
                        copyResult.targetFileFullPath);
                    return result.FailOperation($"Mismatched file metadata for file {file}");
                }

                if(result.Job.DeleteSourceFiles && !_fileStorage.DeleteFile(file))
                {
                    _logger.LogError(
                        "File was copied, but failed to delete source file: {FilePath}",
                        file);
                }

                result.CompleteOperation(copyResult.targetFileFullPath);
                _logger.LogInformation("Copied file: {FilePath}", copyResult.targetFileFullPath);
            }

            _logger.LogInformation("Processed Steam-directory {Directory}", directory);
        }

        return result.CompleteJob();
    }

// Todo: Handle (Conflict) files and dirs. Ignore?
    public async Task<FileTransferJobResult> ExportJottacloudTimelineAsync(CancellationToken ct)
    {
        const string jobKey = "jottacloud_timeline";
        var result = _fileExportJobValidator.ValidateFileTransferJob(jobKey);
        if (result.PreValidationFailed)
        {
            _logger.LogError("Job with key {JobKey} failed pre-validation and cannot be started", jobKey);
            return result;
        }

        result.Start();

        foreach (var file in _fileStorage.EnumerateFiles(result.SourceDirectoryPath))
        {
            result.PrepareOperation(file);
            var timestamp = _fileStorage.GetImageDate(file);
            var structuredDestinationDirectory = GetTargetDirectoryNameFromFileTimestamp(result.Job.TargetDirectoryPath, timestamp);

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

            if (result.Job.DeleteSourceFiles && !_fileStorage.DeleteFile(file))
            {
                _logger.LogError(
                    "File was copied, but failed to delete source file: {FilePath}",
                    file);
            }
            result.CompleteOperation(copyResult.targetFileFullPath);
            _logger.LogInformation("Copied file: {FilePath}", copyResult.targetFileFullPath);
        }

        return result.Complete();
    }

    public string GetTargetDirectoryNameFromFileTimestamp(string destinationRootPath, DateTime fileCreationTime)
    {
        string year = fileCreationTime.Year.ToString();
        int monthIndex = fileCreationTime.Month - 1;
        string monthDirectoryName = GetMonthDirectoryName(monthIndex);

        return Path.Combine(destinationRootPath, year, monthDirectoryName);
    }

    private string GetMonthDirectoryName(int monthIndex)
    {
        if (monthIndex < 0 || monthIndex > 11)
            throw new ArgumentOutOfRangeException(nameof(monthIndex), "Month index must be between 0 and 11");

        string monthName = _culture.DateTimeFormat.MonthNames[monthIndex];
        string capitalizedMonthName = char.ToUpper(monthName[0]) + monthName[1..];

        return $"{monthIndex + 1:D2} {capitalizedMonthName}";
    }
}
