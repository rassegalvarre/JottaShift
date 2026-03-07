namespace JottaShift.Core.FileExport.Jobs.GooglePhotosUpload;

public record GooglePhotosUploadJob : FileExportJob
{
    public required string AlbumName { get; init; }
}