namespace JottaShift.Core.FileExport.Jobs;

public record FileExportJob()
{
    public required string Key { get; init; }
    public required string SourceDirectoryPath { get; init; }
    public required bool DeleteSourceFiles { get; init; }
    public required bool Enabled { get; init; }
}
