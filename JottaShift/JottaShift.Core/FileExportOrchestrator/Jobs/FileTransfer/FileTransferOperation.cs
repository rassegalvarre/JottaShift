using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;

public record FileTransferOperationResult(string SourceFilePath)
{
    public FileTransferOperationStatus Status { get; private set; } = FileTransferOperationStatus.InProgress;
    public string SourceFilePath { get; init; } = SourceFilePath;
    public string? TargetFilePath { get; private set; }
    public bool Success => Status == FileTransferOperationStatus.Completed || Status == FileTransferOperationStatus.TargetExists;

    public void Start()
    {
        Status = FileTransferOperationStatus.InProgress;
    }

    public void TargetExists()
    {
        Status = FileTransferOperationStatus.TargetExists;
    }

    public void Fail(string? targetFilePath)
    {
        TargetFilePath = targetFilePath;
        Status = FileTransferOperationStatus.Failed;
    }

    public void Complete(string targetFilePath)
    {
        Status = FileTransferOperationStatus.Completed;
        TargetFilePath = targetFilePath;
    }
}
