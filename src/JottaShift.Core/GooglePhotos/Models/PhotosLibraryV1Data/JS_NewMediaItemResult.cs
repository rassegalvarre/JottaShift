using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_NewMediaItemResult
{
    [JsonPropertyName("mediaItem")]
    public JS_MediaItem? MediaItem { get; set; }

    [JsonPropertyName("status")]
    public JS_Status? Status { get; set; }

    [JsonPropertyName("uploadToken")]
    public string? UploadToken { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}