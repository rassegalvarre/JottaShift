namespace JottaShift.Core.FileExport.Jobs;

public record FileTransferJob : FileExportJob
{
    public required string SourceDirectoryPath { get; init; }
    public required bool DeleteSourceFiles { get; init; }
    public required string TargetDirectoryPath { get; init; }
}
