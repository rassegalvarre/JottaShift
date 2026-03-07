namespace JottaShift.Core.FileExport.Jobs.FileTransfer;

public record FileTransferJob : FileExportJob
{
    public required string TargetDirectoryPath { get; init; }
}
