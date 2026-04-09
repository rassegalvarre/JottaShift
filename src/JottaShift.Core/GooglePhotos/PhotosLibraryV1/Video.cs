using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record Video
{
    [JsonPropertyName("cameraMake")]
    public string? CameraMake { get; set; }

    [JsonPropertyName("cameraModel")]
    public string? CameraModel { get; set; }

    [JsonPropertyName("fps")]
    public double? Fps { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
