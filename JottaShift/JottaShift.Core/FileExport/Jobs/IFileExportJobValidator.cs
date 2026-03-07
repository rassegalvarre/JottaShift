using JottaShift.Core.FileExport.Jobs.FileTransfer;
using JottaShift.Core.FileExport.Jobs.GooglePhotosUpload;

namespace JottaShift.Core.FileExport.Jobs;

public interface IFileExportJobValidator
{
    bool TryGetFileTransferJob(string key, out FileTransferJob job);
    bool TryGetGooglePhotosUploadJob(string key, out GooglePhotosUploadJob job);

    FileTransferJobResult ValidateFileTransferJob(FileTransferJob job);

    GooglePhotosUploadJobResult ValidateGooglePhotosUploadJob(GooglePhotosUploadJob job);
}
