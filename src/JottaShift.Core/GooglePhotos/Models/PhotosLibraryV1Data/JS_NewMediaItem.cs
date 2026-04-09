using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_NewMediaItem
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("simpleMediaItem")]
    public JS_SimpleMediaItem? SimpleMediaItem { get; set; }
}