using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_CreateAlbumRequest
{
    [JsonPropertyName("album")]
    public JS_Album? Album { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}