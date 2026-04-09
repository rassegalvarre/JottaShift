using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_BatchCreateMediaItemsResponse
{
    [JsonPropertyName("newMediaItemResults")]
    public IList<JS_NewMediaItemResult>? NewMediaItemResults { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}