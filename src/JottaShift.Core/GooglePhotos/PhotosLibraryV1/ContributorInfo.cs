using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public class ContributorInfo
{
    [JsonPropertyName("profilePictureBaseUrl")]
    public string? ProfilePictureBaseUrl { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}