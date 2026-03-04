using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;
using JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;

namespace JottaShift.Core.FileExportOrchestrator;

public class FileExportSettings
{
    public IEnumerable<FileTransferJob> FileTransferJobs { get; init; } = [];

    public IEnumerable<GooglePhotosUploadJob> GooglePhotosUploadJobs { get; init; } = [];
}