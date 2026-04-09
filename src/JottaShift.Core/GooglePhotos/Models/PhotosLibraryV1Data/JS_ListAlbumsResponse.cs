using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_ListAlbumsResponse
{
    [JsonPropertyName("albums")]
    public IList<JS_Album>? Albums { get; set; }

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}