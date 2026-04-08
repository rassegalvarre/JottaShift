namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Defines methods for interacting with the Google Photos API via HTTP
/// </summary>
public interface IGooglePhotosHttpClient
{
    /// <summary>
    /// Uploads an image to Google Photos and returns the upload token.
    /// </summary>
    /// <param name="fileFullPath">Full local path of the file.</param>
    /// <returns>A Result containing the upload token if successful, or an error message if failed.</returns>
    Task<Result<string>> UploadPhotoAsync(string fileFullPath);
}
