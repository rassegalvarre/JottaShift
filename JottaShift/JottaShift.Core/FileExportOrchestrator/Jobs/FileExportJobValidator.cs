using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;
using JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;
using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.FileExportOrchestrator.Jobs;

public class FileExportJobValidator(
    ILogger<FileExportJobValidator> _logger,
    FileExportSettings _fileExportSettings,
    IFileStorage _fileStorage) : IFileExportJobValidator
{
    public bool TryGetFileTransferJob(string key, out FileTransferJob job)
    {
        bool wasFound = false;
        var foundJob = _fileExportSettings.FileTransferJobs.FirstOrDefault(j => j.Key == key);
        
        if (foundJob != null)
        {
            wasFound = true;
            job = foundJob;
        }
        else 
        {
            job = new FileTransferJob()
            {
                Key = string.Empty,
                SourceDirectoryPath = string.Empty,
                TargetDirectoryPath = string.Empty,
                DeleteSourceFiles = false,
                Enabled = false,
            };
        }   

        return wasFound;
    }

    public bool TryGetGooglePhotosUploadJob(string key, GooglePhotosUploadJob job)
    {
        bool wasFound = false;
        var foundJob = _fileExportSettings.GooglePhotosUploadJobs.FirstOrDefault(j => j.Key == key);

        if (foundJob != null)
        {
            wasFound = true;
            job = foundJob;
        }
        else
        {
            job = new GooglePhotosUploadJob()
            {
                Key = string.Empty,
                SourceDirectoryPath = string.Empty,
                AlbumName = string.Empty,
                DeleteSourceFiles = false,
                Enabled = false,
            };
        }

        return wasFound;
    }

    public bool TryGetGooglePhotosUploadJob(string key, out GooglePhotosUploadJob? job)
    {
        throw new NotImplementedException();
    }

    public FileTransferJobResult ValidateFileTransferJob(FileTransferJob job)
    {
        var result = FileTransferJobResult.CreateFromJob(job);
        if (!job.Enabled)
        {
            _logger.LogError("Job with key {JobKey} is diabled and will not be started", job.Key);
            result.Disabled();
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} does not exist",
                job.SourceDirectoryPath);
            result.Invalid();
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.TargetDirectoryPath, true)))
        {
            _logger.LogError(
                "Could not create target directory with name @{DirectoryName}",
                job.TargetDirectoryPath);

            result.Invalid();
        }
        else
        {
            result.Ready(job);
        }

        return result;
    }

    public GooglePhotosUploadJobResult ValidateGooglePhotosUploadJob(GooglePhotosUploadJob job)
    {
        var result = GooglePhotosUploadJobResult.CreateFromJob(job);
        if (!job.Enabled)
        {
            _logger.LogError("Job with key {JobKey} is diabled and will not be started", job.Key);
            result.Disabled();
        }
        else if (!_fileStorage.ValidateDirectory(new DirectoryOptions(job.SourceDirectoryPath, false)))
        {
            _logger.LogError(
                "Source directory with name @{DirectoryName} does not exist",
                job.SourceDirectoryPath);
            result.Invalid();
        }       
        else
        {
            result.Ready(job);
        }

        return result;
    }
}
