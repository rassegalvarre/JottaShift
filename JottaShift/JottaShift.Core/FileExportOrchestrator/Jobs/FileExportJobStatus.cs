namespace JottaShift.Core.FileExportOrchestrator.Jobs;

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