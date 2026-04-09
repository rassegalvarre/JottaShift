using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_MediaMetadata
{
    [JsonPropertyName("creationTime")]
    public string? CreationTime { get; set; }

    [JsonPropertyName("width")]
    public string? Width { get; set; }

    [JsonPropertyName("height")]
    public string? Height { get; set; }

    [JsonPropertyName("photo")]
    public JS_Photo? Photo { get; set; }

    [JsonPropertyName("video")]
    public JS_Video? Video { get; set; }
}