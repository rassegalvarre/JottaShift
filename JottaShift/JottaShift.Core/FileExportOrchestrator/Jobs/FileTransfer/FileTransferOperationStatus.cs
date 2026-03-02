namespace JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;

public enum FileTransferOperationStatus
{
    InProgress,
    TargetExists,
    Failed,
    Completed,
}