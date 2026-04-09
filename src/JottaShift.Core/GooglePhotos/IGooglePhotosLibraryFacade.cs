using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;

namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Facade for Google Photos Library API via <see cref="PhotosLibraryService"/>.
/// Handles service initialization, caching, and provides domain operations.
/// Registered as a singleton for lazy initialization and service reuse.
/// </summary>
[Obsolete("Use IGooglePhotosHttpClient instead")]
public interface IGooglePhotosLibraryFacade
{
    [Obsolete]
    Task<Result<Album>> GetAlbumFromTitleAsync(string albumName);

    [Obsolete]
    Task<Result<Album>> GetAlbumFromIdAsync(string albumId);

    [Obsolete]
    Task<Result<Album>> CreateAlbumAsync(string albumName);

    [Obsolete]
    Task<Result<BatchCreateMediaItemsResponse>> AddImagesToAlbum(string albumId, IEnumerable<string> uploadTokens);
}
