using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record Album
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("productUrl")]
    public string? ProductUrl { get; set; }

    [JsonPropertyName("isWriteable")]
    public bool? IsWriteable { get; set; }

    [JsonPropertyName("shareInfo")]
    public ShareInfo? ShareInfo { get; set; }

    [JsonPropertyName("mediaItemsCount")]
    public string? MediaItemsCount { get; set; }

    [JsonPropertyName("coverPhotoBaseUrl")]
    public string? CoverPhotoBaseUrl { get; set; }

    [JsonPropertyName("coverPhotoMediaItemId")]
    public string? CoverPhotoMediaItemId { get; set; }
}