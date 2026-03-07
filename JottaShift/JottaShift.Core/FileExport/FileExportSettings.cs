using JottaShift.Core.FileExport.Jobs.FileTransfer;
using JottaShift.Core.FileExport.Jobs.GooglePhotosUpload;

namespace JottaShift.Core.FileExport;

public class FileExportSettings
{
    public IEnumerable<FileTransferJob> FileTransferJobs { get; init; } = [];

    public IEnumerable<GooglePhotosUploadJob> GooglePhotosUploadJobs { get; init; } = [];
}