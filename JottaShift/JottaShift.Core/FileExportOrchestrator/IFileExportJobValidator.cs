namespace JottaShift.Core.FileExportOrchestrator;

internal interface IFileExportJobValidator
{
    FileExportJobResult FileTransferJobPreValidation(string jobKey);

    FileExportJobResult GooglePhotosUploadJobPreValidation(string jobKey);
}
