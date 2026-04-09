using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record SimpleMediaItem
{
    [JsonPropertyName("uploadToken")]
    public string? UploadToken { get; set; }

    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
}
