namespace JottaShift.Core.FileExport.Jobs;

public enum FileExportJobStatus
{
    NotStarted,
    Disabled,
    Invalid,
    Ready,
    InProgress,
    Completed,
    Failed,
}