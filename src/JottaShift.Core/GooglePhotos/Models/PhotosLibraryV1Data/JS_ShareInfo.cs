using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_ShareInfo
{
    [JsonPropertyName("sharedAlbumOptions")]
    public JS_SharedAlbumOptions? SharedAlbumOptions { get; set; }

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