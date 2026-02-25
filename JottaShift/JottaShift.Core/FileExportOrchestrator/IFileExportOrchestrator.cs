namespace JottaShift.Core.FileExportOrchestrator;

// TODO: Add method "UploadImagesToGooglePhotosAlbum"
// TODO: Add method "UploadSteamScreenShotsToGamesDirectory"
// TODO: Add method ClearStagedData
// TODO: Rename to StagingService
public interface IFileExportOrchestrator
{
    // TODO: Rename to ExportTimelineBackupToPhotsDirectory
    Task<FileExportResult> ExportAsync(FileExportOptions options, CancellationToken ct = default);
}
