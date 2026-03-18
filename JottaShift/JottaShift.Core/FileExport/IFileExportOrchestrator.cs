using JottaShift.Core.FileExport.Jobs;

namespace JottaShift.Core.FileExport;

public interface IFileExportOrchestrator
{
    Task<FileTransferJobResult> ExportJottacloudTimelineAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default);
    Task<Result> ExportDesktopWallpapersAsync(CancellationToken ct = default);
    Task<Result> ExportChromecastPhotosAsync(CancellationToken ct = default);
}
