namespace JottaShift.Core.FileExport.Jobs;

public enum FileTransferResultStatus
{
    InvalidSourceFile,
    TransferFailed,
    TransferSucceeded,
    NewFileCorrupted
}

public record FileTransferResult : Result
{
    public FileTransferResultStatus Status { get; init; }
    public required string SourceFileFullPath { get; init; }
    public string? NewFileFullPath { get; init; }
    public bool SourceFileDeleted { get; init; } = false;

    public static FileTransferResult Success(
        string sourceFileFullPath,
        string? newFileFullPath = null,
        bool sourceFileDeleted = false) => new()
    {
        Succeeded = true,
        Status = FileTransferResultStatus.TransferSucceeded,
        SourceFileFullPath = sourceFileFullPath,
        NewFileFullPath = newFileFullPath,
        SourceFileDeleted = sourceFileDeleted
    };

    public static FileTransferResult Failure(
        FileTransferResultStatus status,
        string sourceFileFullPath,
        string errorMessage) => new()
    {
        Succeeded = false,
        Status = status,
        SourceFileFullPath = sourceFileFullPath,
        ErrorMessage = errorMessage
    };

    public static FileTransferResult Failure(
        FileTransferResultStatus status,
        string sourceFileFullPath,
        string NewFileFullPath,
        string errorMessage)
    {
        return Failure(status, sourceFileFullPath, errorMessage) with
        {
            NewFileFullPath = NewFileFullPath
        };
    }

    public static FileTransferResult FromFailedResult(
        Result result,
        FileTransferResultStatus status,
        string sourceFileFullPath) => new()
    {
        Succeeded = false,
        Status = status,
        SourceFileFullPath = sourceFileFullPath,
            ErrorMessage = result.ErrorMessage
    };
}