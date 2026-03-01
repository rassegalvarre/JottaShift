namespace JottaShift.Core.FileExportOrchestrator;

public interface IFileExportOrchestrator
{
    Task<FileExportJobResult> ExportJottacloudTimelineAsync(CancellationToken ct = default);
    Task<FileExportJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default);
    Task<FileExportJobResult> ExportDesktopWallpapersAsync(CancellationToken ct = default);
    Task<FileExportJobResult> ExportChromecastPhotosAsync(CancellationToken ct = default);
}
