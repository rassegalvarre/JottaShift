using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record AlbumPosition
{
    [JsonPropertyName("position")]
    public PositionType? Position { get; set; } = PositionType.POSITION_TYPE_UNSPECIFIED;

    [JsonPropertyName("relativeMediaItemId")]
    public string? RelativeMediaItemId { get; set; }

    [JsonPropertyName("relativeEnrichmentItemId")]
    public string? RelativeEnrichmentItemId { get; set; }
}
