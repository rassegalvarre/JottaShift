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
    IFileStorageService _fileStorage,
    IGooglePhotosRepository _googlePhotosRepository,
    IJottacloudRepository _jottacloudRepository,
    ISteamRepository _steamRepository) : IFileExportOrchestrator
{
    #region Directory naming conventions
    private static readonly Dictionary<string, string> ResolutionDirectoryMap = new()
    {
        ["2160"] = "4K",
        ["1440"] = "QHD",
        ["1080"] = "FullHD"
    };

    private static readonly string[] AlphabeticParentDirectoryNames =
    [
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
    #endregion

    private CultureInfo _culture { get; set; } = CultureInfo.CurrentCulture;

    public void SetCulture(CultureInfo culture)
    {
        _culture = culture;
    }

    public async Task<Result> ExportChromecastPhotosAsync(CancellationToken ct = default)
    {
        var job = _fileExportJobs.ChromecastUploadJob;
        if (!job.Enabled)
        {
            _logger.LogInformation("Job {JobId} is disabled and will not run.", job.Id);
            return Result.Success();
        }

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

        var photoUploadResult = await _googlePhotosRepository.UploadPhotosToAlbumAsync(
            job.TargetGooglePhotosAlbumName,
            photosToUpload);
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
        //    string targetDirectoryForResolution = GetDirectoryNameForImageResolution(imageResolution);

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

    public async Task<FileTransferJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default)
    {
        var job = _fileExportJobs.SteamScreenshotsExportJob;

        var validationResult = ValidateFileTransferJob(job);
        if (!validationResult.Succeeded)
        {
            return FileTransferJobResult.FromFailedResult(validationResult, FileTransferJobResultStatus.InvalidJob);
        }

        List<FileTransferResult> fileTransferResults = [];

        foreach (var sourceDirectory in _fileStorage.EnumerateDirectories(job.SourceDirectoryPath))
        {
            var directoryNameResult = _fileStorage.GetDirectoryName(sourceDirectory);
            if (!directoryNameResult.Succeeded || directoryNameResult.Value is null)
            {
                _logger.LogWarning(
                    "Skipping directory with path @{DirectoryPath} since directory name could not be retrieved",
                    sourceDirectory);
                continue;
            }

            var directoryNameToCharArray = directoryNameResult.Value.ToCharArray();
            if (!uint.TryParse(directoryNameToCharArray, out uint appId))
            {
                _logger.LogWarning(
                    "Skipping directory with name @{DirectoryName} since it does not match expected format of a Steam AppId",
                    sourceDirectory);
                continue;
            }

            var appNameResult = await _steamRepository.GetAppNameFromId(appId);
            if (!appNameResult.Succeeded || appNameResult.Value is null)
            {
                _logger.LogWarning("Could not find Steam app name for appId {AppId}", appId);
                continue;
            }
            
            string appName = appNameResult.Value;
            string parentDirectory = GetAlphabeticParentDirectoryName(appName);
            string targetDirectoryForApp = Path.Combine(
                job.TargetDirectoryPath,
                parentDirectory,
                appName);

            foreach (var file in _fileStorage.EnumerateFiles(sourceDirectory))
            {
                FileTransferResult fileTransferResult;
                if (file.Contains("thumbnails"))
                {
                    var deleteThumbnailsResult = DeleteSourceFile(job, file);

                    if (deleteThumbnailsResult.Succeeded)
                    {
                        fileTransferResult = FileTransferResult.Success(file, sourceFileDeleted: true);
                    }
                    else
                    {
                        _logger.LogError("Could not delete thumbnail file with path {FilePath}. Error: {ErrorMessage}",
                            file, deleteThumbnailsResult.ErrorMessage);

                        fileTransferResult = FileTransferResult.FromFailedResult(
                            deleteThumbnailsResult, FileTransferResultStatus.TransferFailed, file);
                    }

                    fileTransferResults.Add(fileTransferResult);
                    continue;
                }

                var copyResult = _fileStorage.CopyFile(file, targetDirectoryForApp);
                if (!copyResult.Succeeded || copyResult.Value is null)
                {
                    _logger.LogError("Could not copy file with path {FilePath}. Error: {ErrorMessage}",
                        file, copyResult.ErrorMessage);

                    fileTransferResult = FileTransferResult.FromFailedResult(
                            copyResult, FileTransferResultStatus.TransferFailed, file);
                    fileTransferResults.Add(fileTransferResult);
                    continue;
                }

                var filesAreBitPerfectMatchResult = _fileStorage.FilesAreBitPerfectMatch(file, copyResult.Value);
                if (!filesAreBitPerfectMatchResult.Succeeded)
                {
                    _logger.LogError(
                        "File was copied, but file content does not match: {FilePath}",
                        copyResult.Value);
                    
                    fileTransferResult = FileTransferResult.FromFailedResult(
                        filesAreBitPerfectMatchResult, FileTransferResultStatus.NewFileCorrupted, file);
                    fileTransferResults.Add(fileTransferResult);
                    continue;
                }

                var metadataMatchResult = _fileStorage.DoesFileMetadataMatch(file, copyResult.Value);
                if (!metadataMatchResult.Succeeded)
                {
                    _logger.LogError("File was copied, but metadata does not match: {FilePath}",
                        copyResult.Value);
                    fileTransferResult = FileTransferResult.FromFailedResult(
                        filesAreBitPerfectMatchResult, FileTransferResultStatus.NewFileCorrupted, file);
                    fileTransferResults.Add(fileTransferResult);
                    continue;
                }

                var deleteSourceResult = DeleteSourceFile(job, file);
                if (!deleteSourceResult.Succeeded)
                {
                    _logger.LogError("Failed to delete source file: {FilePath}", file);
                    continue;
                }

                fileTransferResult = FileTransferResult.Success(file, copyResult.Value, deleteSourceResult.Succeeded);
                fileTransferResults.Add(fileTransferResult);

                _logger.LogInformation("Copied file and deleted source. New path: {CopiedFilePath}",
                    copyResult.Value);
            }

            _logger.LogInformation("Processed Steam-directory {Directory}", sourceDirectory);
        }

        var jobResult = FileTransferJobResult.FromTransferResults(fileTransferResults);
        if (jobResult.Status == FileTransferJobResultStatus.AllFilesTransferredSuccessfully)
        {
            var deleteDirectoryResult = DeleteSourceDirectoryContent(job);
            if (!deleteDirectoryResult.Succeeded)
            {
                _logger.LogError("Failed to delete source directory content: {FilePath}",
                    job.SourceDirectoryPath);
            }

            jobResult.SourceDirectoryDeleted = deleteDirectoryResult.Succeeded;
        }

        return jobResult;
    }

    public async Task<FileTransferJobResult> ExportJottacloudTimelineAsync(CancellationToken ct)
    {
        var job = _fileExportJobs.JottacloudTimelineExportJob;

        var validationResult = ValidateFileTransferJob(job);
        if (!validationResult.Succeeded)
        {
            return FileTransferJobResult.FromFailedResult(validationResult, FileTransferJobResultStatus.InvalidJob);
        }

        List<FileTransferResult> fileTransferResults = [];

        foreach (var file in _fileStorage.EnumerateFiles(job.SourceDirectoryPath))
        {
            FileTransferResult fileTransferResult;

            var imageDateResult = _fileStorage.GetImageDate(file);
            if (!imageDateResult.Succeeded)
            {
                _logger.LogError("Files does not have a valid date property {FilePath}", file);

                fileTransferResult = FileTransferResult.FromFailedResult(
                    imageDateResult, FileTransferResultStatus.InvalidSourceFile, file);
                fileTransferResults.Add(fileTransferResult);
                continue;
            }

            var structuredDestinationDirectory = JottacloudAdapter.PhotoStorageStructuredDirectoryPath(
                imageDateResult.Value, job.TargetDirectoryPath, _culture);

            var cleanedFilePath = JottacloudAdapter.CheckAndCleanConflictedFileName(file);
            var cleanedFileNameResult = _fileStorage.GetFileName(cleanedFilePath);
            if (!cleanedFileNameResult.Succeeded || cleanedFileNameResult.Value == null)
            {
                _logger.LogError("Filename was cleaned but is no longer valid. " +
                    "Previous name: {FileName}. " +
                    "Cleaned name: {FilePath}. " +
                    "Error: {ErrorMessage}",
                    file, cleanedFileNameResult.Value, cleanedFileNameResult.ErrorMessage);

                fileTransferResult = FileTransferResult.FromFailedResult(
                    cleanedFileNameResult, FileTransferResultStatus.InvalidSourceFile, file);
                fileTransferResults.Add(fileTransferResult);
                continue;
            }

            var copyResult = _fileStorage.CopyFile(
                file, structuredDestinationDirectory, cleanedFileNameResult.Value);

            if (!copyResult.Succeeded || copyResult.Value is null)
            {
                _logger.LogError("Could not copy file with path {FilePath}. Error: {ErrorMessage}",
                    file, copyResult.ErrorMessage);

                fileTransferResult = FileTransferResult.FromFailedResult(
                      copyResult, FileTransferResultStatus.TransferFailed, file);
                fileTransferResults.Add(fileTransferResult);
                continue;
            }

            var fileMatchResult = _fileStorage.FilesAreBitPerfectMatch(file, copyResult.Value);
            if (!fileMatchResult.Succeeded)
            {
                _logger.LogError("File was copied, but file content does not match. Copied file: {FilePath}",
                    copyResult.Value);

                fileTransferResult = FileTransferResult.FromFailedResult(
                    fileMatchResult, FileTransferResultStatus.NewFileCorrupted, file);
                fileTransferResults.Add(fileTransferResult);
                continue;
            }

            var metadataMatchResult = _fileStorage.DoesFileMetadataMatch(file, copyResult. Value);
            if (!metadataMatchResult.Succeeded)
            {
                _logger.LogError("File was copied, but metadata does not match. Copied file: {FilePath}",
                    copyResult.Value);

                fileTransferResult = FileTransferResult.FromFailedResult(
                   metadataMatchResult, FileTransferResultStatus.NewFileCorrupted, file);
                fileTransferResults.Add(fileTransferResult);
                continue;
            }


            var deleteSourceResult = DeleteSourceFile(job, file);
            if (!deleteSourceResult.Succeeded)
            {
                _logger.LogError("Failed to delete source file: {FilePath}", file);
                continue;
            }

            fileTransferResult = FileTransferResult.Success(file, copyResult.Value, deleteSourceResult.Succeeded);
            fileTransferResults.Add(fileTransferResult);

            _logger.LogInformation("Copied file and deleted source. New path: {CopiedFilePath}",
                copyResult.Value);
        }

        var jobResult = FileTransferJobResult.FromTransferResults(fileTransferResults);
        if (jobResult.Status == FileTransferJobResultStatus.AllFilesTransferredSuccessfully)
        {
            var deleteDirectoryResult = DeleteSourceDirectoryContent(job);
            if (!deleteDirectoryResult.Succeeded)
            {
                _logger.LogError("Failed to delete source directory content: {FilePath}",
                    job.SourceDirectoryPath);
            }

            jobResult.SourceDirectoryDeleted = deleteDirectoryResult.Succeeded;
        }

        return jobResult;
    }

    public static Result<string> GetDirectoryNameForImageResolution(string imageResolution)
    {
        if (string.IsNullOrWhiteSpace(imageResolution))
        {
            return Result<string>.Failure("Resolution cannot be null or empty");
        }

        var match = ResolutionDirectoryMap
            .FirstOrDefault(kvp => imageResolution.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase));

        return !string.IsNullOrEmpty(match.Value)
            ? Result<string>.Success(match.Value)
            : Result<string>.Failure($"Unknown resolution: {imageResolution}");
    }

    public static string GetAlphabeticParentDirectoryName(string directoryName)
    {      
        string firstLetter = directoryName.ToCharArray()[0]
            .ToString();

        return AlphabeticParentDirectoryNames
            .FirstOrDefault(n => n.StartsWith(firstLetter)) ?? AlphabeticParentDirectoryNames[0];
    }

    private Result DeleteSourceFile(FileTransferJob job, string sourceFilePath)
    {
        if (job.DeleteSourceFiles)
        {
            return _fileStorage.DeleteFile(sourceFilePath);            
        }

        return Result.Success();
    }

    private Result DeleteSourceDirectoryContent(FileTransferJob job)
    {
        if (job.DeleteSourceFiles)
        {
            return _fileStorage.DeleteDirectoryContent(job.SourceDirectoryPath);
        }

        return Result.Success();
    }

    private Result ValidateFileTransferJob(FileTransferJob job)
    {
        if (!job.Enabled)
        {
            _logger.LogWarning("Job with key {JobId} is disabled and will not be started", job.Id);
            return Result.Failure("Job is disabled");
        }
        else if (!_fileStorage.ValidateDirectory(job.SourceDirectoryPath).Succeeded)
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} for job with id {JobId} does not exist",
                job.SourceDirectoryPath,
                job.Id);
            return Result.Failure("Source directory does not exist");
        }
        else if (!_fileStorage.ValidateDirectory(job.TargetDirectoryPath).Succeeded)
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
