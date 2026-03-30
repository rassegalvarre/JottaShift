using Google.Apis.PhotosLibrary.v1.Data;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Implementation of <see cref="IGooglePhotosLibraryFacade"/> using REST API.
/// </summary>
internal class GooglePhotosLibraryRestFacade(
    IUserCredentialManager _userCredentialManager,
    ILogger<GooglePhotosLibraryRestFacade> _logger) : IGooglePhotosLibraryFacade
{
    public Task<Result<BatchCreateMediaItemsResponse>> AddImagesToAlbum(string albumId, IEnumerable<string> uploadTokens)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Album>> CreateAlbumAsync(string albumName)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Album>> GetAlbumFromIdAsync(string albumId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Album>> GetAlbumFromTitleAsync(string albumName)
    {
        throw new NotImplementedException();
    }
}
