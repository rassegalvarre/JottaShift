using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public class JS_BatchCreateMediaItemsResponse
{
    [JsonPropertyName("newMediaItemResults")]
    public IList<JS_NewMediaItemResult>? NewMediaItemResults { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}