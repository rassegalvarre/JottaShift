using System.Text.Json.Serialization;

namespace JottaShift.Core.Jottacloud.Models.Domain;

// Response from "https://api.jottacloud.com/photos/v1/public/<ALBUM_ID>/?order=ASC&limit=<LIMIT>"
public record Album
{
    [JsonPropertyName("collectionType")]
    public int CollectionType { get; set; }

    [JsonPropertyName("commentsGroupId")]
    public string CommentsGroupId { get; set; } = string.Empty;

    [JsonPropertyName("coverPhoto")]
    public CoverPhoto CoverPhoto { get; set; } = new();

    [JsonPropertyName("createdDate")]
    public long CreatedDate { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("lastModified")]
    public long LastModified { get; set; }

    [JsonPropertyName("maxCapturedDate")]
    public long MaxCapturedDate { get; set; }

    [JsonPropertyName("minCapturedDate")]
    public long MinCapturedDate { get; set; }

    [JsonPropertyName("photos")]
    public IEnumerable<Photo> Photos { get; set; } = Enumerable.Empty<Photo>();

    [JsonPropertyName("shareInfo")]
    public ShareInfo ShareInfo { get; set; } = new();

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("bytes")]
    public long Bytes { get; set; }

    [JsonPropertyName("encoded_content_ref")]
    public string EncodedContentRef { get; set; } = string.Empty;
}
