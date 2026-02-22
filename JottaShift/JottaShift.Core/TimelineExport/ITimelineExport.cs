namespace JottaShift.Core.TimelineExport;

// TODO: Add method "UploadImagesFromStagingToGooglePhotosAlbum"
// TODO: Add method "UploadSteamScreenShotsToGamesFolder"
// TODO: Add method ClearStagedData
// TODO: Rename to StagingService
public interface ITimelineExport
{
    // TODO: Rename to ExportTimelineBackupToPhotsDirectory
    Task<TimelineExportResult> ExportAsync(TimelineExportOptions options, CancellationToken ct = default);
}
