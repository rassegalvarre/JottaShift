
using System.Text.Json.Serialization;

namespace JottaShift.Core.Jottacloud;

// Response from "https://api.jottacloud.com/photos/v1/public/<ALBUM_ID>/?order=ASC&limit=<LIMIT>"
#region Root object
public class GetAlbumResponse
{
    [JsonPropertyName("collectionType")]
    public int CollectionType { get; set; }

    [JsonPropertyName("commentsGroupId")]
    public string CommentsGroupId { get; set; } = string.Empty;

    [JsonPropertyName("coverPhoto")]
    public CoverPhoto CoverPhoto { get; set; } = new();

    [JsonPropertyName("createdDate")]
    public long CreatedDate { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("lastModified")]
    public long LastModified { get; set; }

    [JsonPropertyName("maxCapturedDate")]
    public long MaxCapturedDate { get; set; }

    [JsonPropertyName("minCapturedDate")]
    public long MinCapturedDate { get; set; }

    [JsonPropertyName("photos")]
    public IEnumerable<Photo> Photos { get; set; } = Enumerable.Empty<Photo>();

    [JsonPropertyName("shareInfo")]
    public ShareInfo ShareInfo { get; set; } = new();

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("bytes")]
    public long Bytes { get; set; }

    [JsonPropertyName("encoded_content_ref")]
    public string EncodedContentRef { get; set; } = string.Empty;
}
#endregion

#region Nested objects
public class CoverPhoto
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

public class OwnerAvatar
{
    [JsonPropertyName("profilePhotoUrl")]
    public string ProfilePhotoUrl { get; set; } = string.Empty;

    [JsonPropertyName("initials")]
    public string Initials { get; set; } = string.Empty;

    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; } = string.Empty;
}

public class Photo
{
    // All fields are identical to those in CoverPhoto – reuse the same class definition.
    // To keep the model clear we inherit from CoverPhoto (they share the same schema).
    // If you prefer composition, copy the properties instead of inheriting.
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

public class ShareInfo
{
    [JsonPropertyName("admin")]
    public bool Admin { get; set; }

    [JsonPropertyName("authorization")]
    public string Authorization { get; set; } = string.Empty;

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("ownerFullName")]
    public string OwnerFullName { get; set; } = string.Empty;

    [JsonPropertyName("ownerAvatar")]
    public OwnerAvatar OwnerAvatar { get; set; } = new();

    [JsonPropertyName("shareDate")]
    public long ShareDate { get; set; }

    [JsonPropertyName("subscribers")]
    public IEnumerable<object> Subscribers { get; set; } = Enumerable.Empty<object>();

    [JsonPropertyName("subscriptionDate")]
    public long SubscriptionDate { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}
#endregion