using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_ContributorInfo
{
    [JsonPropertyName("profilePictureBaseUrl")]
    public string? ProfilePictureBaseUrl { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}