using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

public class JS_Video
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