namespace JottaShift.Core.FileExport.Jobs.FileTransfer;

public enum FileTransferOperationStatus
{
    InProgress,
    TargetExists,
    Failed,
    Completed,
}