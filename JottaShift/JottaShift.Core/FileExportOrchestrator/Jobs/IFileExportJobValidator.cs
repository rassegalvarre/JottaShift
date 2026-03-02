using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;
using JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;

namespace JottaShift.Core.FileExportOrchestrator.Jobs;

public interface IFileExportJobValidator
{
    FileTransferJobResult ValidateFileTransferJob(string jobKey);

    GooglePhotosUploadJobResult ValidateGooglePhotosUploadJob(string jobKey);
}
