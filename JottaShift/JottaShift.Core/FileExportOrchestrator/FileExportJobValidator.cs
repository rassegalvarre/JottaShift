using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.FileExportOrchestrator;

internal class FileExportJobValidator(
    ILogger<FileExportJobValidator> _logger,
    FileExportSettings _fileExportSettings,
    IFileStorage _fileStorage) : IFileExportJobValidator
{
    public FileTransferJob? GetFileTransferJob(string key)
    {
        return _fileExportSettings.FileTransferJobs.FirstOrDefault(j => j.Key == key);
    }

    private GooglePhotosUploadJob? GetGooglePhotosUploadJob(string key)
    {
        return _fileExportSettings.GooglePhotosUploadJobs.FirstOrDefault(j => j.Key == key);
    }

    private FileExportJobResult FileExportJobPreValidation(FileExportJob job)
    {
        FileExportJobResult result;
        if (!job.Enabled)
        {
            _logger.LogError("Job with key {JobKey} is diabled and will not be started", job.Key);
            result = FileExportJobResult.Disabled(job.Key);
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} does not exist",
                job.SourceDirectoryPath);
            result = FileExportJobResult.Invalid(job.Key, "Missing source directory");
        }
        else
        {
            result = FileExportJobResult.Ready(job);
        }

        return result;
    }

    public FileExportJobResult FileTransferJobPreValidation(string jobKey)
    {
        var job = GetFileTransferJob(jobKey);
        if (job == null)
        {
            return FileExportJobResult.Invalid(jobKey, "Job not found");
        }

        var preValidation = FileExportJobPreValidation(job);
        if (preValidation.PreValidationFailed)
        {
            return preValidation;
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.TargetDirectoryPath, true)))
        {
            _logger.LogError(
                "Could not create target directory with name @{DirectoryName}",
                job.TargetDirectoryPath);

            return FileExportJobResult.Invalid(job.Key, "Missing target directory");
        }

        return FileExportJobResult.Ready(job);
    }

    public FileExportJobResult GooglePhotosUploadJobPreValidation(string jobKey)
    {
        var job = GetGooglePhotosUploadJob(jobKey);
        if (job == null)
        {
            return FileExportJobResult.Invalid(jobKey, "Job not found");
        }

        return FileExportJobPreValidation(job);
    }
}
