using JottaShift.Core.FileExport.Jobs.FileTransfer;
using JottaShift.Core.FileExport.Jobs.GooglePhotosUpload;
using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.FileExport.Jobs;

public class FileExportJobValidator(
    ILogger<FileExportJobValidator> _logger,
    FileExportJobs _fileExportSettings,
    IFileStorage _fileStorage) : IFileExportJobValidator
{

    // Move to orchestrator
    public Result<bool> ValidateFileTransferJob(FileTransferJob job)
    {
        if (!job.Enabled)
        {
            _logger.LogInformation("Job with key {JobKey} is diabled and will not be started", job.Key);
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
}
