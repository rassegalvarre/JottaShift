namespace JottaShift.Core.FileExportOrchestrator;

public interface IFileExportOrchestrator
{
    Task<FileExportResult> ExportJottacloudTimelineAsync(FileExportOptions options, CancellationToken ct = default);
    Task<FileExportResult> ExportSteamScreenshotsAsync(FileExportOptions options, CancellationToken ct = default);
    Task<FileExportResult> ExportDesktopWallpapers4kAsync(FileExportOptions options, CancellationToken ct = default);
    Task<FileExportResult> ExportDesktopWallpapersWQHDAsync(FileExportOptions options, CancellationToken ct = default);
    Task<FileExportResult> ExportChromecastPhotosAsync(FileExportOptions options, CancellationToken ct = default);
}
