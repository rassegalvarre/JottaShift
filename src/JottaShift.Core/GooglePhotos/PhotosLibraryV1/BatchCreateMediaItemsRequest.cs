using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public class BatchCreateMediaItemsRequest
{
    [JsonPropertyName("albumId")]
    public string? AlbumId { get; set; }

    [JsonPropertyName("newMediaItems")]
    public IList<NewMediaItem>? NewMediaItems { get; set; }

    [JsonPropertyName("albumPosition")]
    public AlbumPosition? AlbumPosition { get; set; }
}