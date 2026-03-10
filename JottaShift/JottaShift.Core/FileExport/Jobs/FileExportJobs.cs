namespace JottaShift.Core.FileExport.Jobs;

public class FileExportJobs
{
    public required ChromecastUploadJob ChromecastUploadJob { get; init; }
    public required FileTransferJob JottacloudTimelineExportJob { get; init; }
    public required FileTransferJob SteamScreenshotsExportJob { get; init; }
    public required FileTransferJob ScreenshotsExportJob { get; init; }
}