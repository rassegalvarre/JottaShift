using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public class BatchCreateMediaItemsResponse
{
    [JsonPropertyName("newMediaItemResults")]
    public IList<NewMediaItemResult>? NewMediaItemResults { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}