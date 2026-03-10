namespace JottaShift.Core.FileExport.Jobs;

public abstract record FileExportJob()
{
    public required string Id { get; init; }
    public required bool Enabled { get; init; }
}