namespace JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;

public record GooglePhotosUploadJob : FileExportJob
{
    public required string AlbumName { get; init; }
}