using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_SimpleMediaItem
{
    [JsonPropertyName("uploadToken")]
    public string? UploadToken { get; set; }

    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
}