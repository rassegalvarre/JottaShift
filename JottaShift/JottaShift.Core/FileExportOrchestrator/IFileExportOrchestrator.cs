namespace JottaShift.Core.FileExportOrchestrator;

public interface IFileExportOrchestrator
{
    Task<FileExportResult> ExportJottacloudTimelineAsync(CancellationToken ct = default);
    Task<FileExportResult> ExportSteamScreenshotsAsync(CancellationToken ct = default);
    Task<FileExportResult> ExportDesktopWallpapers4kAsync(CancellationToken ct = default);
    Task<FileExportResult> ExportDesktopWallpapersWQHDAsync(CancellationToken ct = default);
    Task<FileExportResult> ExportChromecastPhotosAsync(CancellationToken ct = default);
}
