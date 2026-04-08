using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.Domain;

public record GooglePhotosAlbum
{
    [JsonPropertyName("coverPhotoBaseUrl")]
    public required string CoverPhotoBaseUrl { get; set; }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("isWriteable")]
    public required bool IsWriteable { get; set; } = false;
    
    [JsonPropertyName("productUrl")]
    public required string ProductUrl { get; set; }    

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("totalMediaItems")]
    public long? TotalMediaItems { get; set; } = 0;
}
