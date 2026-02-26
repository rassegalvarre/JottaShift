namespace JottaShift.Core.FileExportOrchestrator;

public enum FileTransferOperationResultStatus
{
    Invalid,
    NotStarted,
    InProgress,
    Completed,
    Failed
}

public record FileTransferOperationResult(string SourceFilePath)
{
    public FileTransferOperationResultStatus Status { get; private set; } = FileTransferOperationResultStatus.NotStarted;
    public string SourceFilePath { get; init; } = SourceFilePath;
    public string? TargetFilePath { get; private set; }
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

    public void Fail(string? targetFilePath)
    {
        TargetFilePath = targetFilePath;
        Status = FileTransferOperationResultStatus.Failed;
    }

    public void Complete(string? targetFilePath)
    {
        if (string.IsNullOrEmpty(targetFilePath))
        {
            throw new ArgumentException("Target file path cannot be null or empty when completing the file transfer operation.", nameof(targetFilePath));
        }
        TargetFilePath = targetFilePath;
        Status = FileTransferOperationResultStatus.Completed;
    }
}