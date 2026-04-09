using JottaShift.Core.GooglePhotos.Models.Domain;
using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryApi;

// TODO: Rename to "CreateAlbumRequest" when Google.Apis.PhotosLibrary.v1 is removed
public record AlbumsCreateRequest
{
    [JsonPropertyName("album")]
    public required GooglePhotosAlbum Album { get; init; }
}
