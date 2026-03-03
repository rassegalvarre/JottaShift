using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;
using JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;

namespace JottaShift.Core.FileExportOrchestrator;

public interface IFileExportOrchestrator
{
    Task<FileTransferJobResult> ExportJottacloudTimelineAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportDesktopWallpapersAsync(CancellationToken ct = default);
    Task<GooglePhotosUploadJobResult> ExportChromecastPhotosAsync(CancellationToken ct = default);
}
