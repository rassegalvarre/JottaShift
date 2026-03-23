using JottaShift.Core.FileExport.Jobs;
using JottaShift.Core.GooglePhotos;

namespace JottaShift.Core.FileExport;

public interface IFileExportOrchestrator
{
    Task<FileTransferJobResult> ExportJottacloudTimelineAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportDesktopWallpapersAsync(CancellationToken ct = default);
    Task<AlbumUploadResult> ExportChromecastPhotosAsync(CancellationToken ct = default);
}
