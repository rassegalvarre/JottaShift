namespace JottaShift.Core.FileExportOrchestrator;

public interface IFileExportOrchestrator
{
    Task<FileTransferJobResult> ExportJottacloudTimelineAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportDesktopWallpapers4kAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportDesktopWallpapersWQHDAsync(CancellationToken ct = default);
    Task<GooglePhotosUploadJobResult> ExportChromecastPhotosAsync(CancellationToken ct = default);
}
