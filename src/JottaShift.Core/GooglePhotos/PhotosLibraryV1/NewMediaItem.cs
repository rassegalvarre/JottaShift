using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record NewMediaItem
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("simpleMediaItem")]
    public SimpleMediaItem? SimpleMediaItem { get; set; }
}
