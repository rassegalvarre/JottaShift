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

    #region FileExportJobs
    public async Task<AlbumUploadResult> ExportChromecastPhotosAsync(CancellationToken ct = default)
    {
        var job = _fileExportJobs.ChromecastUploadJob;
        if (!job.Enabled)
        {
            _logger.LogInformation("Job {JobId} is disabled and will not run.", job.Id);
            return AlbumUploadResult.Success(job.TargetGooglePhotosAlbumName, []);
        }

        var stagingAlbumResult = await _jottacloudRepository.GetAlbumAsync(
            job.SourceJottacloudAlbumId);
        if (!stagingAlbumResult.Succeeded || stagingAlbumResult.Value?.Photos == null)
        {
            _logger.LogInformation("Could not get album with id {AlbumId} for staged Chromecast photos", 
                job.SourceJottacloudAlbumId);;
            return AlbumUploadResult.FromFailedResult(stagingAlbumResult, job.TargetGooglePhotosAlbumName);
        }

        var photosToUpload = stagingAlbumResult.Value.Photos
            .Where(p => !string.IsNullOrEmpty(p.LocalFilePath))
            .Select(p => p.LocalFilePath!);

        var photoUploadResult = await _googlePhotosRepository.UploadPhotosToAlbumAsync(
            job.TargetGooglePhotosAlbumName,
            photosToUpload);

        return photoUploadResult;
    }

    public async Task<FileTransferJobResult> ExportDesktopWallpapersAsync(CancellationToken ct = default)
    {
        var job = _fileExportJobs.ScreenshotsExportJob;

        var validationResult = ValidateFileTransferJob(job);
        if (!validationResult.Succeeded)
        {
            return FileTransferJobResult.FromFailedResult(validationResult, FileTransferJobResultStatus.InvalidJob);
        }

        List<FileTransferResult> fileTransferResults = [];

        foreach (var file in _fileStorage.EnumerateFiles(job.SourceDirectoryPath))
        {
            FileTransferResult fileTransferResult;

            var imageResolutionResult = _fileStorage.GetImageResolution(file);
            if (!imageResolutionResult.Succeeded || imageResolutionResult.Value is null)
            {
                _logger.LogWarning(
                    "Skipping file with path @{FilePath} since image resolution could not be retrieved",
                    file);

                fileTransferResult = FileTransferResult.FromFailedResult(
                    imageResolutionResult, FileTransferResultStatus.InvalidSourceFile, file);
                fileTransferResults.Add(fileTransferResult);
                continue;
            }

            var targetDirectoryResult = GetDirectoryNameForImageResolution(imageResolutionResult.Value);
            if (!targetDirectoryResult.Succeeded || targetDirectoryResult.Value is null)
            {
                _logger.LogWarning(
                    "Skipping file with path @{FilePath} since it does not match expected resolutions",
                    file);

                fileTransferResult = FileTransferResult.FromFailedResult(
                    imageResolutionResult, FileTransferResultStatus.InvalidSourceFile, file);
                fileTransferResults.Add(fileTransferResult);
                continue;
            }
            
            string fullTargetDirectoryPath = Path.Combine(job.TargetDirectoryPath, targetDirectoryResult.Value);

            var copyResult = _fileStorage.CopyFile(file, fullTargetDirectoryPath);

            fileTransferResult = PostFileCopyValidation(job, file, copyResult);
            fileTransferResults.Add(fileTransferResult);         
        }

        return PostFileTransferValidation(job, fileTransferResults);
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
            string cleanedSourceDirectory = JottacloudAdapter.CheckAndCleanConflictedFileName(
                sourceDirectory);
            var directoryNameResult = _fileStorage.GetDirectoryName(cleanedSourceDirectory);
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
            
            string appName = _fileStorage.SanitizeStringToValidDirectoryName(appNameResult.Value);
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
                        fileTransferResult = FileTransferResult.Success(file, "File is excluded from transfer", true);
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

                fileTransferResult = PostFileCopyValidation(job, file, copyResult);
                fileTransferResults.Add(fileTransferResult);
            }

            _logger.LogInformation("Processed Steam-directory {Directory}", sourceDirectory);
        }

        return PostFileTransferValidation(job, fileTransferResults);
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

            fileTransferResult = PostFileCopyValidation(job, file, copyResult);
            fileTransferResults.Add(fileTransferResult);
        }
        
        _logger.LogInformation("Processed Jottacloud timeline directory {Directory}", job.SourceDirectoryPath);

        return PostFileTransferValidation(job, fileTransferResults);
    }
    #endregion

    #region Static helper methods for directory naming
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
    #endregion

    #region Cleanup of staging resources
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
    #endregion

    #region Pre and post job validation
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

    private FileTransferResult PostFileCopyValidation(
        FileTransferJob job,
        string sourceFilePath,
        Result<string> copyResult)
    {
        if (!copyResult.Succeeded || copyResult.Value is null)
        {
            _logger.LogError("Could not copy file with path {FilePath}. Error: {ErrorMessage}",
                sourceFilePath, copyResult.ErrorMessage);

            return FileTransferResult.FromFailedResult(
                  copyResult, FileTransferResultStatus.TransferFailed, sourceFilePath);
        }

        var fileMatchResult = _fileStorage.FilesAreBitPerfectMatch(sourceFilePath, copyResult.Value);
        if (!fileMatchResult.Succeeded)
        {
            _logger.LogError("File was copied, but file content does not match. Copied file: {FilePath}",
                copyResult.Value);

            return FileTransferResult.FromFailedResult(
                fileMatchResult, FileTransferResultStatus.NewFileCorrupted, sourceFilePath);
        }

        var metadataMatchResult = _fileStorage.DoesFileMetadataMatch(sourceFilePath, copyResult.Value);
        if (!metadataMatchResult.Succeeded)
        {
            _logger.LogError("File was copied, but metadata does not match. Copied file: {FilePath}",
                copyResult.Value);

            return FileTransferResult.FromFailedResult(
               metadataMatchResult, FileTransferResultStatus.NewFileCorrupted, sourceFilePath);
        }

        var deleteSourceResult = DeleteSourceFile(job, sourceFilePath);
        if (!deleteSourceResult.Succeeded)
        {
            _logger.LogError("File was copied, but failed to delete source file: {FilePath}",
                sourceFilePath);
        }
        else
        {
            _logger.LogInformation("Copied file and deleted source. New path: {CopiedFilePath}",
                copyResult.Value);
        }

        return FileTransferResult.Success(sourceFilePath, copyResult.Value, deleteSourceResult.Succeeded);
    }

    private FileTransferJobResult PostFileTransferValidation(FileTransferJob job, List<FileTransferResult> fileTransferResults)
    {
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
    #endregion
}
