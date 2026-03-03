using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;
using JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;

namespace JottaShift.Core.FileExportOrchestrator.Jobs;

public interface IFileExportJobValidator
{
    bool TryGetFileTransferJob(string key, out FileTransferJob job);
    bool TryGetGooglePhotosUploadJob(string key, out GooglePhotosUploadJob job);

    FileTransferJobResult ValidateFileTransferJob(FileTransferJob job);

    GooglePhotosUploadJobResult ValidateGooglePhotosUploadJob(GooglePhotosUploadJob job);
}
