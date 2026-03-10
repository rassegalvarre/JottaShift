using System.Text.Json.Serialization;

namespace JottaShift.Core.Jottacloud.Models.Domain;

public record Photo
{
    [JsonPropertyName("camera")]
    public string Camera { get; set; } = string.Empty;

    [JsonPropertyName("capturedDate")]
    public long CapturedDate { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("commentsItemId")]
    public string CommentsItemId { get; set; } = string.Empty;

    [JsonPropertyName("exposure")]
    public string Exposure { get; set; } = string.Empty;

    [JsonPropertyName("focalLength")]
    public string FocalLength { get; set; } = string.Empty;

    [JsonPropertyName("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("filesize")]
    public long Filesize { get; set; }

    [JsonPropertyName("geoAddress")]
    public string GeoAddress { get; set; } = string.Empty;

    [JsonPropertyName("geoHash")]
    public string GeoHash { get; set; } = string.Empty;

    [JsonPropertyName("gpsCoords")]
    public string GpsCoords { get; set; } = string.Empty;

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("hidden")]
    public bool Hidden { get; set; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("iso")]
    public int Iso { get; set; }

    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = string.Empty;

    [JsonPropertyName("mimetype")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("ownerFullName")]
    public string OwnerFullName { get; set; } = string.Empty;

    [JsonPropertyName("ownerAvatar")]
    public OwnerAvatar OwnerAvatar { get; set; } = new();

    [JsonPropertyName("thumbnail_url")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("encoded_content_ref")]
    public string EncodedContentRef { get; set; } = string.Empty;
}
