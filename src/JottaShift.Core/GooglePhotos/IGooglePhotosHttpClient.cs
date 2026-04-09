using JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;

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
    Task<Result<JS_BatchCreateMediaItemsResponse>> BatchAddMediaItemsAsync(string albumId, IEnumerable<string> uploadTokens);

    /// <summary>
    /// Creates a new album.
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/create">
    /// Method: albums.create</see>
    /// </summary>
    Task<Result<JS_Album>> CreateAlbumAsync(string albumTitle);

    /// <summary>
    /// Gets an album by its ID.
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/get">Method: albums.get</see>
    /// </summary>
    Task<Result<JS_Album>> GetAlbumAsync(string albumId);

    Task<Result<JS_Album>> GetAlbumFromTitleAsync(string albumTitle);

    /// <summary>
    /// Uploads media to Google storage.
    /// <see href="https://developers.google.com/photos/library/guides/upload-media">Upload media docs</see>.
    /// </summary>
    /// <returns>Upload token</returns>
    Task<Result<string>> UploadMediaAsync(string fileFullPath);

}
