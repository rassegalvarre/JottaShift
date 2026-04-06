using Google.Apis.PhotosLibrary.v1.Data;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Implementation of <see cref="IGooglePhotosLibraryFacade"/> using REST API.
/// TODO: Merge with IGooglePhotosHttpClient
/// </summary>
internal class GooglePhotosLibraryRestFacade(
    IUserCredentialManager _userCredentialManager,
    IHttpClientWrapper httpClientWrapper,
    ILogger<GooglePhotosLibraryRestFacade> _logger) : IGooglePhotosLibraryFacade
{
    // https://developers.google.com/photos/library/reference/rest/v1/albums/batchAddMediaItems
    public Task<Result<BatchCreateMediaItemsResponse>> AddImagesToAlbum(string albumId, IEnumerable<string> uploadTokens)
    {
        throw new NotImplementedException();
    }

    // https://developers.google.com/photos/library/reference/rest/v1/albums/create
    public Task<Result<Album>> CreateAlbumAsync(string albumName)
    {
        throw new NotImplementedException();
    }

    // https://developers.google.com/photos/library/reference/rest/v1/albums/get
    public Task<Result<Album>> GetAlbumFromIdAsync(string albumId)
    {
        throw new NotImplementedException();
    }

    // https://developers.google.com/photos/library/reference/rest/v1/albums/list
    public Task<Result<Album>> GetAlbumFromTitleAsync(string albumName)
    {
        throw new NotImplementedException();
    }
}
