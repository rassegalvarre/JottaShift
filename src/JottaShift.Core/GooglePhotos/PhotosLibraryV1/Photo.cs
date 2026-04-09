using System.Text.Json.Serialization;

namespace JottaShift.Core.GooglePhotos.PhotosLibraryV1;

public record Photo
{
    [JsonPropertyName("cameraMake")]
    public string? CameraMake { get; set; }

    [JsonPropertyName("cameraModel")]
    public string? CameraModel { get; set; }

    [JsonPropertyName("focalLength")]
    public double? FocalLength { get; set; }

    [JsonPropertyName("apertureFNumber")]
    public double? ApertureFNumber { get; set; }

    [JsonPropertyName("isoEquivalent")]
    public int? IsoEquivalent { get; set; }

    [JsonPropertyName("exposureTime")]
    public string? ExposureTime { get; set; }
}
