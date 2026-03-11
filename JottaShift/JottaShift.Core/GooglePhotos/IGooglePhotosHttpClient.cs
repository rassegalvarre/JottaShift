namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Defines methods for interacting with the Google Photos API via HTTP
/// </summary>
public interface IGooglePhotosHttpClient
{
    /// <summary>
    /// Uploads an image to Google Photos and returns the upload token.
    /// </summary>
    /// <param name="fileName">The name of the file to upload.</param>
    /// <param name="fileData">The byte array of the file data.</param>
    /// <returns>A Result containing the upload token if successful, or an error message if failed.</returns>
    Task<Result<string>> UploadPhoto(string fileName, byte[] fileData);
}
