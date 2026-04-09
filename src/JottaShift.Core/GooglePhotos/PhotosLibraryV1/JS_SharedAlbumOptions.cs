using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public class JS_SharedAlbumOptions
{
    [JsonPropertyName("isCollaborative")]
    public bool? IsCollaborative { get; set; }

    [JsonPropertyName("isCommentable")]
    public bool? IsCommentable { get; set; }
}