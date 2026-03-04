namespace JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;

public record FileTransferJob : FileExportJob
{
    public required string TargetDirectoryPath { get; init; }
}
