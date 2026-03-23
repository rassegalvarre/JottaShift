using System.Text.Json.Serialization;

namespace JottaShift.Core.Jottacloud.Models.Domain;

public record OwnerAvatar
{
    [JsonPropertyName("profilePhotoUrl")]
    public string ProfilePhotoUrl { get; set; } = string.Empty;

    [JsonPropertyName("initials")]
    public string Initials { get; set; } = string.Empty;

    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = string.Empty;
}
