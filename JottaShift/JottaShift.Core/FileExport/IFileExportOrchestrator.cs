namespace JottaShift.Core.FileExport;

public interface IFileExportOrchestrator
{
    Task<Result> ExportJottacloudTimelineAsync(CancellationToken ct = default);
    Task<Result> ExportSteamScreenshotsAsync(CancellationToken ct = default);
    Task<Result> ExportDesktopWallpapersAsync(CancellationToken ct = default);
    Task<Result> ExportChromecastPhotosAsync(CancellationToken ct = default);
}
