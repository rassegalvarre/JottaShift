using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;

namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Facade for Google Photos Library API via <see cref="PhotosLibraryService"/>.
/// Handles service initialization, caching, and provides domain operations.
/// Registered as a singleton for lazy initialization and service reuse.
/// </summary>
public interface IGooglePhotosLibraryFacade
{
    Task<Result<Album>> GetAlbumAsync(string albumName);
}
