using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace JottaShift.Core.FileExportOrchestrator;

public sealed class FileExportOrchestrator(
    FileExportSettings _fileExportSettings,
    ILogger<FileExportOrchestrator> _logger,
    IFileStorage _fileStorage,
    IGooglePhotosRepository _googlePhotosRepository,
    ISteamRepository _steamRepository) : IFileExportOrchestrator
{
    private CultureInfo _culture { get; set; } = CultureInfo.CurrentCulture;

    public void SetCulture(CultureInfo culture)
    {
        _culture = culture;
    }

    // Todo: Handle (Conflict) files and dirs. Ignore?
    public async Task<FileExportJobResult> ExportChromecastPhotosAsync(CancellationToken ct = default)
    {
        const string jobKey = "chromecast_photos";
        var job = GetGooglePhotosUploadJob(jobKey);
        if (job == null)
        {
            _logger.LogError("No file transfer job setting found with key: {JobKey}", jobKey);
            return FileExportJobResult.Invalid(jobKey, "Missing job setting");
        }

        if (!job.Enabled)
        {
            _logger.LogError("Job with key {JobKey} is diabled and will not be started", jobKey);
            return FileExportJobResult.Disabled(job.Key);
        }

        if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} does not exist",
                job.SourceDirectoryPath);

            return FileExportJobResult.Invalid(jobKey, "Missing source directory");
        }

        var result = FileExportJobResult.StartJob(job);

        List<string> filePathsToUpload = [.. _fileStorage.EnumerateFiles(job.SourceDirectoryPath)];
        if (filePathsToUpload.Count == 0)
        {
            _logger.LogInformation("No images staged for upload to Google Photos");
            return result.CompleteJob();
        }

        var filesUploaded = await _googlePhotosRepository.UploadImagesToAlbum(filePathsToUpload, job.AlbumName);

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

    public async Task<FileExportJobResult> ExportDesktopWallpapersAsync(CancellationToken ct = default)
    {
        const string jobKey = "desktop_wallpapers";

        var job = GetFileTransferJob(jobKey);
        if (job == null)
        {
            _logger.LogError("No file transfer job setting found with key: {JobKey}", jobKey);
            return FileExportJobResult.Invalid(jobKey, "Missing job setting");
        }

        if (!job.Enabled)
        {
            _logger.LogError("Job with key {JobKey} is diabled and will not be started", jobKey);
            return FileExportJobResult.Disabled(job.Key);
        }

        if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} does not exist",
                job.SourceDirectoryPath);

            return FileExportJobResult.Invalid(jobKey, "Missing source directory");
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.TargetDirectoryPath, true)))
        {
            _logger.LogError(
                "Could not create target directory with name @{DirectoryName}",
                job.TargetDirectoryPath);

            return FileExportJobResult.Invalid(jobKey, "Missing target directory");
        }

        var result = FileExportJobResult.StartJob(job);

        foreach (var file in _fileStorage.EnumerateFiles(job.SourceDirectoryPath))
        {
            result.PrepareOperation(file);

            // Check resolution
            // Move file to correct subdirectory based on resolution (e.g. 4K, 1080p, etc.)
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
           var fullTargetDirectoryPath = Path.Combine(job.TargetDirectoryPath, targetDirectoryForResolution);

            var copyResult = await _fileStorage.CopyAsync(file, job.TargetDirectoryPath, false, ct);
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
            if (job.DeleteSourceFiles && !_fileStorage.DeleteFile(file))
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

    public async Task<FileExportJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default)
    {
        const string jobKey = "steam_screenshots";

        var job = GetFileTransferJob(jobKey);
        if (job == null)
        {
            _logger.LogError("No file transfer job setting found with key: {JobKey}", jobKey);
            return FileExportJobResult.Invalid(jobKey, "Missing job setting");
        }

        if (!job.Enabled)
        {
            _logger.LogError("Job with key {JobKey} is diabled and will not be started", jobKey);
            return FileExportJobResult.Disabled(job.Key);
        }

        if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} does not exist",
                job.SourceDirectoryPath);

            return FileExportJobResult.Invalid(jobKey, "Missing source directory");
        }

        var result = FileExportJobResult.StartJob(job);
        foreach (var directory in _fileStorage.EnumerateDirectories(job.SourceDirectoryPath))
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

            string targetDirectoryForApp = Path.Combine(job.TargetDirectoryPath, appName);

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

                if(job.DeleteSourceFiles && !_fileStorage.DeleteFile(file))
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
    public async Task<FileExportJobResult> ExportJottacloudTimelineAsync(CancellationToken ct)
    {
        const string jobKey = "jottacloud_timeline";

        var job = GetFileTransferJob(jobKey);
        if (job == null)
        {
            _logger.LogError("No file transfer job setting found with key: {JobKey}", jobKey);
            return FileExportJobResult.Invalid(jobKey, "Missing job setting");
        }

        if (!job.Enabled)
        {
            _logger.LogError("Job with key {JobKey} is diabled and will not be started", jobKey);
            return FileExportJobResult.Disabled(job.Key);
        }

        if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} does not exist",
                job.SourceDirectoryPath);

            return FileExportJobResult.Invalid(jobKey, "Missing source directory");
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.TargetDirectoryPath, true)))
        {
            _logger.LogError(
                "Could not create target directory with name @{DirectoryName}",
                job.TargetDirectoryPath);

            return FileExportJobResult.Invalid(jobKey, "Missing target directory");
        }

        var result = FileExportJobResult.StartJob(job);

        foreach (var file in _fileStorage.EnumerateFiles(job.SourceDirectoryPath))
        {
            result.PrepareOperation(file);
            var timestamp = _fileStorage.GetImageDate(file);
            var structuredDestinationDirectory = GetTargetDirectoryNameFromFileTimestamp(job.TargetDirectoryPath, timestamp);

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

            if (job.DeleteSourceFiles && !_fileStorage.DeleteFile(file))
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

    public FileTransferJob? GetFileTransferJob(string key)
    {
        return _fileExportSettings.FileTransferJobs.FirstOrDefault(j => j.Key == key);
    }

    public GooglePhotosUploadJob? GetGooglePhotosUploadJob(string key)
    {
        return _fileExportSettings.GooglePhotosUploadJobs.FirstOrDefault(j => j.Key == key);
    }
}
