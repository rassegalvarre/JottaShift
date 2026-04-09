using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public class JS_CreateAlbumRequest
{
    [JsonPropertyName("album")]
    public JS_Album? Album { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}