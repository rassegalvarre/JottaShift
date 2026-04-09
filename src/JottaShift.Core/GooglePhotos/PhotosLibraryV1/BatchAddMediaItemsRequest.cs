using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record BatchAddMediaItemsRequest
{
    [JsonPropertyName("mediaItemIds")]
    public IList<string> MediaItemIds { get; init; } = [];
}