using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record SharedAlbumOptions
{
    [JsonPropertyName("isCollaborative")]
    public bool? IsCollaborative { get; set; }

    [JsonPropertyName("isCommentable")]
    public bool? IsCommentable { get; set; }
}
