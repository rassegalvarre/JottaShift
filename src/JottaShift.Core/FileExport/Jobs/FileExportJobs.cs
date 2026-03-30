namespace JottaShift.Core.FileExport.Jobs;

public class FileExportJobs
{
    public required bool SaveJobResultsToFile { get; init; } = false;
    public required GooglePhotosUploadJob ChromecastUploadJob { get; init; }
    public required FileTransferJob JottacloudTimelineExportJob { get; init; }
    public required FileTransferJob SteamScreenshotsExportJob { get; init; }
    public required FileTransferJob DesktopWallpaperExportJob { get; init; }
}