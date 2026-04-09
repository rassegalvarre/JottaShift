using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record CreateAlbumRequest
{
    [JsonPropertyName("album")]
    public Album? Album { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}
