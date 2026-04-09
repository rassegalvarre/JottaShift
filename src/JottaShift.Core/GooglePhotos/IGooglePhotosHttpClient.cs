namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Defines methods for interacting with the Google Photos API via HTTP
/// </summary>
public interface IGooglePhotosHttpClient
{
    /// <summary>
    /// Uploads media to Google storage.
    /// <see href="https://developers.google.com/photos/library/guides/upload-media">Upload media docs</see>.
    /// </summary>
    /// <returns>Upload token</returns>
    Task<Result<string>> UploadMediaAsync(string fileFullPath);
}
