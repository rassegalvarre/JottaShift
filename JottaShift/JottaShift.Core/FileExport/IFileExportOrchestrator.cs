using JottaShift.Core.FileExport.Jobs.FileTransfer;
using JottaShift.Core.FileExport.Jobs.GooglePhotosUpload;

namespace JottaShift.Core.FileExport;

public interface IFileExportOrchestrator
{
    Task<FileTransferJobResult> ExportJottacloudTimelineAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportSteamScreenshotsAsync(CancellationToken ct = default);
    Task<FileTransferJobResult> ExportDesktopWallpapersAsync(CancellationToken ct = default);
    Task<GooglePhotosUploadJobResult> ExportChromecastPhotosAsync(CancellationToken ct = default);
}
