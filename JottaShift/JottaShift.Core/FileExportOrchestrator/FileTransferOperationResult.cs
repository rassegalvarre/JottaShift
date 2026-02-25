namespace JottaShift.Core.FileExportOrchestrator;

public enum FileTransferOperationResultStatus
{
    NotStarted,
    InProgress,
    TransferFailed,
    InvalidFileContent,
    InvalidFileMetadata,
    Completed,
}

public record FileTransferOperationResult(string SourceFilePath)
{
    public FileTransferOperationResultStatus Status { get; private set; } = FileTransferOperationResultStatus.NotStarted;
    public string SourceFilePath { get; init; } = SourceFilePath;
    public string TargetFilePath { get; private set; } = string.Empty;
    public bool Success { get; init; }

    private static FileTransferOperationResult CreateFromStatus(string SourceFilePath, FileTransferOperationResultStatus status)
    {
        return new FileTransferOperationResult(SourceFilePath)
        {
            Status = status
        };
    }

    public static FileTransferOperationResult Prepare(string sourceFilePath)
    {
        return CreateFromStatus(sourceFilePath, FileTransferOperationResultStatus.NotStarted);
    }

    public void Start()
    {
        Status = FileTransferOperationResultStatus.InProgress;
    }

    public void TransferEnded(string targetFilePath)
    {
        TargetFilePath = targetFilePath;
    }

    public void TransferFailed(string targetFilePath)
    {
        TargetFilePath = targetFilePath;
        Status = FileTransferOperationResultStatus.TransferFailed;
    }

    public void InvalidFileContent()
    {
        Status = FileTransferOperationResultStatus.InvalidFileContent;
    }

    public void InvalidMetadata()
    {
        Status = FileTransferOperationResultStatus.InvalidFileMetadata;
    }

    public void Complete()
    {
        Status = FileTransferOperationResultStatus.Completed;
    }
}