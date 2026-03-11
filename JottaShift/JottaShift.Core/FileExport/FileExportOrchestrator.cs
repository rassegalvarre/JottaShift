using JottaShift.Core.FileExport.Jobs;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.Steam;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace JottaShift.Core.FileExport;

public sealed class FileExportOrchestrator(
    FileExportJobs _fileExportJobs,
    ILogger<FileExportOrchestrator> _logger,
    IFileStorage _fileStorage,
    IGooglePhotosRepository _googlePhotosRepository,
    IJottacloudRepository _jottacloudRepository,
    ISteamRepository _steamRepository) : IFileExportOrchestrator
{
    private CultureInfo _culture { get; set; } = CultureInfo.CurrentCulture;

    public void SetCulture(CultureInfo culture)
    {
        _culture = culture;
    }

    public async Task<Result> ExportChromecastPhotosAsync(CancellationToken ct = default)
    {
        var job = _fileExportJobs.ChromecastUploadJob;
        var stagingAlbumResult = await _jottacloudRepository.GetAlbumAsync(
            job.SourceJottacloudAlbumId);
        if (!stagingAlbumResult.Succeeded || stagingAlbumResult.Value?.Photos == null)
        {
            _logger.LogInformation("Could not get album with id {AlbumId} for staged Chromecast photos", 
                job.SourceJottacloudAlbumId);;
            return Result.Failure("Could not get album");
        }

        var photosToUpload = stagingAlbumResult.Value.Photos
            .Where(p => !string.IsNullOrEmpty(p.LocalFilePath))
            .Select(p => p.LocalFilePath!);

        var photoUploadResult = await _googlePhotosRepository.UploadPhotosToAlbum(
            photosToUpload,
            job.TargetGooglePhotosAlbumName);
        if (!photoUploadResult.Succeeded || photoUploadResult.Value is 0)
        {
            return Result.Failure($"No files were uploaded to Google");
        }

        if (photoUploadResult.Value != photosToUpload.Count())
        {
            _logger.LogError(
                "Did not upload all images to Google: {FilesUploaded} out of {FileCount} were uploaded",
                photoUploadResult, photosToUpload.Count());
            return Result.Failure("Some photos were not uploaded");
        }

        return Result.Success();
    }

    public async Task<Result> ExportDesktopWallpapersAsync(CancellationToken ct = default)
    {
        return Result.Failure("Not implemented");

        //const string jobKey = DefaultJobKeys.DesktopWallpapers;
        //FileTransferJobResult result;

        //if (!_fileExportJobValidator.TryGetFileTransferJob(jobKey, out var job))
        //{
        //    _logger.LogError("Job with key {JobKey} does not exists", jobKey);
        //    result = new FileTransferJobResult(jobKey);
        //    result.Invalid();
        //    return result;
        //}

        //result = _fileExportJobValidator.ValidateFileTransferJob(job);
        //if (result.PreValidationFailed)
        //{
        //    _logger.LogError("Job with key {JobKey} failed pre-validation and cannot be started", jobKey);
        //    return result;
        //}

        //result.Start();

        //foreach (var file in _fileStorage.EnumerateFiles(job.SourceDirectoryPath))
        //{
        //    var imageResolution = _fileStorage.GetImageResolution(file);
        //    string targetDirectoryForResolution;

        //    if (imageResolution.EndsWith("1440"))
        //    {
        //        targetDirectoryForResolution = "QHD";
        //    }
        //    else if (imageResolution.EndsWith("2160"))
        //    {
        //        targetDirectoryForResolution = "4K";
        //    }
        //    else if (imageResolution.EndsWith("1080"))
        //    {
        //        targetDirectoryForResolution = "FullHD";
        //    }
        //    else
        //    {
        //        _logger.LogWarning(
        //            "Skipping file with name @{FileName} since it does not match expected format of ending with resolution",
        //            file);

        //        continue;
        //    }

        //    result.PrepareOperation(file);

        //    string fullTargetDirectoryPath = Path.Combine(job.TargetDirectoryPath, targetDirectoryForResolution);

        //    result.StartOperation();
        //    var copyResult = await _fileStorage.CopyAsync(file, fullTargetDirectoryPath, ct);
        //    if (!copyResult.Success)
        //    {
        //        return result.FailOperation($"File transfer failed for file {file}");
        //    }
        //    if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
        //    {
        //        _logger.LogError(
        //            "File was copied, but file content does not match: {FilePath}",
        //            copyResult.targetFileFullPath);
        //        return result.FailOperation($"Mismatched file content for file {file}");
        //    }
        //    if (!_fileStorage.DoesFileMetadataMatch(file, copyResult.targetFileFullPath))
        //    {
        //        _logger.LogError(
        //            "File was copied, but metadata does not match: {FilePath}",
        //            copyResult.targetFileFullPath);
        //        return result.FailOperation($"Mismatched file metadata for file {file}");
        //    }

        //    DeleteSourceFile(file, job);

        //    result.CompleteOperation(copyResult.targetFileFullPath);
        //    _logger.LogInformation("Copied file: {FilePath}", copyResult.targetFileFullPath);
        //}

        //DeleteSourceDirectory(job, result);

        //return result.Complete();
    }

    public async Task<Result> ExportSteamScreenshotsAsync(CancellationToken ct = default)
    {
        return Result.Failure("Not implemented");

        //var job = _
        //FileTransferJobResult result;

        //if (!_fileExportJobValidator.TryGetFileTransferJob(jobKey, out var job))
        //{
        //    _logger.LogError("Job with key {JobKey} does not exists", jobKey);
        //    result = new FileTransferJobResult(jobKey);
        //    result.Invalid();
        //    return result;
        //}

        //result = _fileExportJobValidator.ValidateFileTransferJob(job);
        //if (result.PreValidationFailed)
        //{
        //    _logger.LogError("Job with key {JobKey} failed pre-validation and cannot be started", jobKey);
        //    return result;
        //}

        //result.Start();
        //foreach (var directory in _fileStorage.EnumerateDirectories(job.SourceDirectoryPath))
        //{
        //    var directoryNameToCharArray = Path.GetFileName(directory)?.ToCharArray();
        //    if (!uint.TryParse(directoryNameToCharArray, out uint appId))
        //    {
        //        _logger.LogWarning(
        //            "Skipping directory with name @{DirectoryName} since it does not match expected format of a Steam appId",
        //            directory);
        //        continue;
        //    }

        //    var appName = await _steamRepository.GetAppNameFromId(appId);

        //    if (string.IsNullOrEmpty(appName))
        //    {
        //        _logger.LogWarning("Could not find Steam app name for appId {AppId}", appId);
        //        continue;
        //    }

            //string parentDirectory = GetAlphabeticParentDirectoryName(appName);
            //string targetDirectoryForApp = Path.Combine(
            //    job.TargetDirectoryPath,
            //    parentDirectory,
            //    appName);

            //if (!_fileStorage.ValidateDirectory(new DirectoryOptions(targetDirectoryForApp, true)))
            //{
            //    _logger.LogError(
            //        "Could not create target directory with name @{DirectoryName}",
            //        targetDirectoryForApp);

            //    return result.FailOperation("Cannot create target directory");
            //}

            //foreach (var file in _fileStorage.EnumerateFiles(directory))
            //{
            //    if (file.Contains("thumbnails"))
            //    {
            //        DeleteSourceFile(file, job);
            //        continue;
            //    }

            //    result.PrepareOperation(file);
            //    var copyResult = await _fileStorage.CopyAsync(file, targetDirectoryForApp, ct);
            //    if (!copyResult.Success)
            //    {
            //        return result.FailOperation($"File transfer failed for file {file}");
            //    }
            //    if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
            //    {
            //        _logger.LogError(
            //            "File was copied, but file content does not match: {FilePath}",
            //            copyResult.targetFileFullPath);
            //        return result.FailOperation($"Mismatched file content for file {file}");
            //    }
            //    if (!_fileStorage.DoesFileMetadataMatch(file, copyResult.targetFileFullPath))
            //    {
            //        _logger.LogError(
            //            "File was copied, but metadata does not match: {FilePath}",
            //            copyResult.targetFileFullPath);
            //        return result.FailOperation($"Mismatched file metadata for file {file}");
            //    }

            //    DeleteSourceFile(file, job);

            //    result.CompleteOperation(copyResult.targetFileFullPath);
            //    _logger.LogInformation("Copied file: {FilePath}", copyResult.targetFileFullPath);
            //}

        //    _logger.LogInformation("Processed Steam-directory {Directory}", directory);
        //}
        
        //DeleteSourceDirectory(job, result);

        //return result.Complete();
    }

    // Todo: Handle (Conflict) files and dirs. Ignore?
    public async Task<Result> ExportJottacloudTimelineAsync(CancellationToken ct)
    {
        return Result.Failure("Not implemened");
        //const string jobKey = DefaultJobKeys.JottacloudTimeline;
        //FileTransferJobResult result;

        //if (!_fileExportJobValidator.TryGetFileTransferJob(jobKey, out var job))
        //{
        //    _logger.LogError("Job with key {JobKey} does not exists", jobKey);
        //    result = new FileTransferJobResult(jobKey);
        //    result.Invalid();
        //    return result;
        //}

        //result = _fileExportJobValidator.ValidateFileTransferJob(job);
        //if (result.PreValidationFailed)
        //{
        //    _logger.LogError("Job with key {JobKey} failed pre-validation and cannot be started", jobKey);
        //    return result;
        //}

        //result.Start();

        //foreach (var file in _fileStorage.EnumerateFiles(result.SourceDirectoryPath))
        //{
        //    result.PrepareOperation(file);
        //    var timestamp = _fileStorage.GetImageDate(file);
        //    var structuredDestinationDirectory = JottacloudAdapter.PhotoStorageStructuredDirectoryPath(timestamp, job.TargetDirectoryPath, CultureInfo.CurrentCulture);

        //    result.StartOperation();
            
        //    var copyResult = await _fileStorage.CopyAsync(file, structuredDestinationDirectory, ct);

        //    if (!copyResult.Success)
        //    {
        //        return result.FailOperation($"File transfer failed");
        //    }

        //    if (!_fileStorage.FilesAreBitPerfectMatch(file, copyResult.targetFileFullPath))
        //    {
        //        _logger.LogError(
        //            "File was copied, but file content does not match: {FilePath}",
        //            copyResult.targetFileFullPath);
        //        return result.FailOperation($"Mismatched file content");
        //    }

        //    if (!_fileStorage.DoesFileMetadataMatch(file, copyResult.targetFileFullPath))
        //    {
        //        _logger.LogError(
        //            "File was copied, but metadata does not match: {FilePath}",
        //            copyResult.targetFileFullPath);
        //        return result.FailOperation($"Mismatched file metadata");
        //    }

        //    DeleteSourceFile(file, job);

        //    result.CompleteOperation(copyResult.targetFileFullPath);
        //    _logger.LogInformation("Copied file: {FilePath}", copyResult.targetFileFullPath);
        //}

        //DeleteSourceDirectory(job, result);

        //return result.Complete();
    }

    public string GetAlphabeticParentDirectoryName(string directoryName)
    {
        string[] alphabeticParentDirectoryNames = [
            "0 - Numerisk",
            "A - Alpha",
            "B - Bravo",
            "C - Charlie",
            "D - Delta",
            "E - Echo",
            "F - Foxtrot",
            "G - Golf",
            "H - Hotel",
            "I - India",
            "J - Juliett",
            "K - Kilo",
            "L - Lima",
            "M - Mike",
            "N - November",
            "O - Oscar",
            "P - Papa",
            "Q - Quebec",
            "R - Romeo",
            "S - Sierra",
            "T - Tango",
            "U - Uniform",
            "V - Victor",
            "W - Whiskey",
            "X - X‑ray",
            "Y - Yankee",
            "Z - Zulu"
        ];

        string firstLetter = directoryName.ToCharArray()[0]
            .ToString();

        return alphabeticParentDirectoryNames
            .FirstOrDefault(n => n.StartsWith(firstLetter)) ?? alphabeticParentDirectoryNames[0];
    }

    private void DeleteSourceFile(FileTransferJob job, string sourceFilePath)
    {
        if (job.DeleteSourceFiles)
        {
            bool deleted = _fileStorage.DeleteFile(sourceFilePath);
            if (!deleted)
            {
                _logger.LogError(
                    "Failed to delete source file: {FilePath}",
                    sourceFilePath);
            }
        }
    }

    private void DeleteSourceDirectory(FileTransferJob job, Result result)
    {
        if (job.DeleteSourceFiles && result.Succeeded)
        {
            bool deleted = _fileStorage.DeleteDirectoryContent(job.SourceDirectoryPath);
            if (!deleted)
            {
                _logger.LogError(
                    "Failed to delete source directory contents: {FilePath}",
                    job.SourceDirectoryPath);
            }
        }
    }

    private Result ValidateFileTransferJob(FileTransferJob job)
    {
        if (!job.Enabled)
        {
            _logger.LogWarning("Job with key {JobId} is disabled and will not be started", job.Id);
            return Result.Failure("Job is disabled");
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} for job with id {JobId} does not exist",
                job.SourceDirectoryPath,
                job.Id);
            return Result.Failure("Source directory does not exist");
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.TargetDirectoryPath, true)))
        {
            _logger.LogError(
                "Could not create target directory with name @{DirectoryName} for job with id {JobId}",
                job.TargetDirectoryPath,
                job.Id);
            return Result.Failure("Target directory does not exist");
        }

        return Result.Success();
    }
}
