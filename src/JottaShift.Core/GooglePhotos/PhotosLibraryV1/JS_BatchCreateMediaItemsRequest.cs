using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public class JS_BatchCreateMediaItemsRequest
{
    [JsonPropertyName("albumId")]
    public string? AlbumId { get; set; }

    [JsonPropertyName("newMediaItems")]
    public IList<JS_NewMediaItem>? NewMediaItems { get; set; }
}