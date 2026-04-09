using JottaShift.Core.GooglePhotos.Models.Domain;
using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.Api;

public record AlbumsResponse
{
    [JsonPropertyName("albums")]
    public required IEnumerable<GooglePhotosAlbum> Albums { get; set; }
}
