namespace JottaShift.Core.FileExport.Jobs;

public abstract record FileExportJob()
{
    public required bool Enabled { get; init; }
}