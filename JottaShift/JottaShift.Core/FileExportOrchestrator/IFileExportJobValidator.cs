namespace JottaShift.Core.FileExportOrchestrator;

public interface IFileExportJobValidator
{
    FileExportJobResult FileTransferJobPreValidation(string jobKey);

    FileExportJobResult GooglePhotosUploadJobPreValidation(string jobKey);
}
