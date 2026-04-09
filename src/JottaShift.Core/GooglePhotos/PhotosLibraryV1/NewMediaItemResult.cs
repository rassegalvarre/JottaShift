using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record NewMediaItemResult
{
    [JsonPropertyName("mediaItem")]
    public MediaItem? MediaItem { get; set; }

    [JsonPropertyName("status")]
    public Status? Status { get; set; }

    [JsonPropertyName("uploadToken")]
    public string? UploadToken { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}
