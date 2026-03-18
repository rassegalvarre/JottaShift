namespace JottaShift.Core.FileExport.Jobs;

public enum FileTransferJobResultStatus
{
    InvalidJob,
    NoFilesToTransfer,
    AllFilesTransferredSuccessfully,
    SomeFilesFailedToTransfer,
}

public record FileTransferJob : FileExportJob
{
    public required string SourceDirectoryPath { get; init; }
    public required bool DeleteSourceFiles { get; init; }
    public required string TargetDirectoryPath { get; init; }
}

public record FileTransferJobResult : Result<IEnumerable<FileTransferResult>>
{
    public FileTransferJobResultStatus Status { get; set; }
    public bool SourceDirectoryDeleted { get; set; } = false;

    public static FileTransferJobResult Success(
        FileTransferJobResultStatus status,
        IEnumerable<FileTransferResult> fileTransferResults,
        bool sourceDirectoryDeleted = false) => new()
    {
        Status = status,
        Value = fileTransferResults,
        SourceDirectoryDeleted = sourceDirectoryDeleted,
        Succeeded = status == FileTransferJobResultStatus.NoFilesToTransfer ||
            status == FileTransferJobResultStatus.AllFilesTransferredSuccessfully,
    };

    public static FileTransferJobResult Failure(FileTransferJobResultStatus status, string errorMessage) => new()
    {
        Succeeded = false,
        Status = status,
        Value = [],
        ErrorMessage = errorMessage
    };

    public static FileTransferJobResult FromFailedResult(Result result, FileTransferJobResultStatus status) => new()
    {
        Succeeded = false,
        Status = status,
        Value = [],
        ErrorMessage = result.ErrorMessage
    };

    public static FileTransferJobResult FromTransferResults(IEnumerable<FileTransferResult> transferResults)
    {
        if (!transferResults.Any())
        {
            return Success(FileTransferJobResultStatus.NoFilesToTransfer, transferResults);
        }

        if (transferResults.All(r => r.Status == FileTransferResultStatus.TransferSucceeded))
        {
            return Success(FileTransferJobResultStatus.AllFilesTransferredSuccessfully, transferResults);
        }

        return Failure(FileTransferJobResultStatus.SomeFilesFailedToTransfer, "Some files failed to transfer.") with
        {
            Value = transferResults
        };
    }
}