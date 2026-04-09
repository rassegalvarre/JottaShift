using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.Domain;

// TODO: Rename to "Album" when Google.Apis.PhotosLibrary.v1 is removed
public record GooglePhotosAlbum
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("coverPhotoBaseUrl")]
    public string? CoverPhotoBaseUrl { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("productUrl")]
    public string? ProductUrl { get; set; }
    
    [JsonPropertyName("isWriteable")]
    public bool? IsWriteable { get; set; } = false;
    
    [JsonPropertyName("totalMediaItems")]
    public long? TotalMediaItems { get; set; } = 0;
}
