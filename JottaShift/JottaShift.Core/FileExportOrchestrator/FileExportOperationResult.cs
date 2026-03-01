namespace JottaShift.Core.FileExportOrchestrator;

public enum FileExportOperationResultStatus
{
    Invalid,
    NotStarted,
    InProgress,
    Completed,
    Failed
}

public record FileExportOperationResult(string SourceFilePath)
{
    public FileExportOperationResultStatus Status { get; private set; } = FileExportOperationResultStatus.NotStarted;
    public string SourceFilePath { get; init; } = SourceFilePath;
    public string? TargetFilePath { get; private set; }
    public bool Success { get; init; }

    private static FileExportOperationResult CreateFromStatus(string SourceFilePath, FileExportOperationResultStatus status)
    {
        return new FileExportOperationResult(SourceFilePath)
        {
            Status = status
        };
    }

    public static FileExportOperationResult Prepare(string sourceFilePath)
    {
        return CreateFromStatus(sourceFilePath, FileExportOperationResultStatus.NotStarted);
    }

    public void Start()
    {
        Status = FileExportOperationResultStatus.InProgress;
    }

    public void Fail(string? targetFilePath)
    {
        TargetFilePath = targetFilePath;
        Status = FileExportOperationResultStatus.Failed;
    }

    public void Complete(string? targetFilePath)
    {
        if (string.IsNullOrEmpty(targetFilePath))
        {
            throw new ArgumentException("Target file path cannot be null or empty when completing the file transfer operation.", nameof(targetFilePath));
        }
        TargetFilePath = targetFilePath;
        Status = FileExportOperationResultStatus.Completed;
    }
}