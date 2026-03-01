namespace JottaShift.Core.FileExportOrchestrator;

public interface IFileExportOrchestrator
{
    Task<FileExportJobResult> ExportJottacloudTimelineAsync(CancellationToken ct = default);
    Task<FileExportJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default);
    Task<FileExportJobResult> ExportDesktopWallpapers4kAsync(CancellationToken ct = default);
    Task<FileExportJobResult> ExportDesktopWallpapersWQHDAsync(CancellationToken ct = default);
    Task<GooglePhotosUploadJobResult> ExportChromecastPhotosAsync(CancellationToken ct = default);
}
