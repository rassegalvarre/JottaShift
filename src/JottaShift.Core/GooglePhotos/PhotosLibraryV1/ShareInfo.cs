using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record ShareInfo
{
    [JsonPropertyName("sharedAlbumOptions")]
    public SharedAlbumOptions? SharedAlbumOptions { get; set; }

    [JsonPropertyName("shareableUrl")]
    public string? ShareableUrl { get; set; }

    [JsonPropertyName("shareToken")]
    public string? ShareToken { get; set; }

    [JsonPropertyName("isJoined")]
    public bool? IsJoined { get; set; }

    [JsonPropertyName("isOwned")]
    public bool? IsOwned { get; set; }

    [JsonPropertyName("isJoinable")]
    public bool? IsJoinable { get; set; }
}
