using JottaShift.Core.GooglePhotos.PhotosLibraryV1;

namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Defines methods for interacting with the Google Photos API via HTTP.
/// <see href="https://developers.google.com/photos/library/guides/get-started-library">
/// Get started with the Library API</see>
/// </summary>
public interface IGooglePhotosLibraryHttpClient
{
    /// <summary>
    /// Adds a collection of media items to an album using their upload tokens.
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/batchAddMediaItems">
    /// Method: albums.batchAddMediaItems</see>
    /// </summary>
    Task<Result<BatchCreateMediaItemsResponse>> BatchAddMediaItemsAsync(string albumId, IEnumerable<string> uploadTokens);

    /// <summary>
    /// Creates a new album.
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/create">
    /// Method: albums.create</see>
    /// </summary>
    Task<Result<Album>> CreateAlbumAsync(string albumTitle);

    /// <summary>
    /// Gets an album by its ID.
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/get">
    /// Method: albums.get</see>
    /// </summary>
    Task<Result<Album>> GetAlbumAsync(string albumId);

    /// <summary>
    /// Get an album byt its title.
    /// Method is internal and not part of the official Google Photos API.
    /// </summary>
    Task<Result<Album>> GetAlbumFromTitleAsync(string albumTitle);

    /// <summary>
    /// List all available albums.
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/list">
    /// Method: albums.list</see>
    /// </summary>
    Task<Result<ListAlbumsResponse>> ListAlbumsAsync();

    /// <summary>
    /// Uploads media to Google storage.
    /// <see href="https://developers.google.com/photos/library/guides/upload-media">
    /// Upload media docs</see>.
    /// </summary>
    /// <returns>Upload token</returns>
    Task<Result<string>> UploadMediaAsync(string fileFullPath);

}
