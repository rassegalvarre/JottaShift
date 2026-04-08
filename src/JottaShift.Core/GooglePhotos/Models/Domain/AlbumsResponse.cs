using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.Domain;

public record AlbumsResponse
{
    [JsonPropertyName("albums")]
    public required IEnumerable<AlbumResponse> Albums { get; set; }
}
