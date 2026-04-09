using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public class JS_MediaItem
{
    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("contributorInfo")]
    public JS_ContributorInfo? ContributorInfo { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("mediaMetadata")]
    public JS_MediaMetadata? MediaMetadata { get; set; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("productUrl")]
    public string? ProductUrl { get; set; }

    [JsonPropertyName("ETag")]
    public string? ETag { get; set; }
}