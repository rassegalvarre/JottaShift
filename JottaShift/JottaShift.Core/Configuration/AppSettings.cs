using JottaShift.Core.FileExportOrchestrator;

namespace JottaShift.Core.Configuration;

public class AppSettings
{
    public required FileExportSettings FileExportSettings { get; init; }
}
