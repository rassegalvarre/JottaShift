using System.Text.Json.Serialization;

namespace JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;

public record GooglePhotosUploadJob : FileExportJob
{
    [JsonPropertyName("album_name")]
    public required string AlbumName { get; init; }
}