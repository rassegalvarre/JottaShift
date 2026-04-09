using Google.Apis.PhotosLibrary.v1.Data;
using JottaShift.Core.GooglePhotos.Models.Domain;

namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Defines methods for interacting with the Google Photos API via HTTP
/// </summary>
public interface IGooglePhotosHttpClient
{
    /// <summary>
    /// Adds a collection of media items to an album using their upload tokens.
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/batchAddMediaItems">
    /// Method: albums.batchAddMediaItems</see>
    /// </summary>
    /// <returns></returns>
    Task<Result<BatchCreateMediaItemsResponse>> BatchAddMediaItemsAsync(string albumId, IEnumerable<string> uploadTokens);

    /// <summary>
    /// Creates a new album.
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/create">
    /// Method: albums.create</see>
    /// </summary>
    Task<Result<GooglePhotosAlbum>> CreateAlbumAsync(string albumName);

    /// <summary>
    /// Gets an album by its ID.
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/get">Method: albums.get</see>
    /// </summary>
    Task<Result<GooglePhotosAlbum>> GetAlbumAsync(string albumId);

    /// <summary>
    /// Uploads media to Google storage.
    /// <see href="https://developers.google.com/photos/library/guides/upload-media">Upload media docs</see>.
    /// </summary>
    /// <returns>Upload token</returns>
    Task<Result<string>> UploadMediaAsync(string fileFullPath);

}
