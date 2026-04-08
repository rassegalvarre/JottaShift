using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.GooglePhotos;

/// <summary>
/// Implementation of <see cref="IGooglePhotosLibraryFacade"/> using REST API.
/// TODO: Merge with IGooglePhotosHttpClient
/// </summary>
public class GooglePhotosLibraryRestFacade(
    IUserCredentialManager _userCredentialManager,
    IGooglePhotosHttpClient _httpClientWrapper,
    ILogger<GooglePhotosLibraryRestFacade> _logger) : IGooglePhotosLibraryFacade
{
    // https://developers.google.com/photos/library/reference/rest/v1/albums/batchAddMediaItems
    public async Task<Result<BatchCreateMediaItemsResponse>> AddImagesToAlbum(string albumId, IEnumerable<string> uploadTokens)
    {
        var batchCreateRequest = new BatchCreateMediaItemsRequest
        {
            AlbumId = albumId,
            NewMediaItems = [.. uploadTokens.Select(token => new NewMediaItem
                {
                    SimpleMediaItem = new SimpleMediaItem { UploadToken = token }
                })]
        };

        string requestUri = $"https://photoslibrary.googleapis.com/v1/albums/{albumId}:batchAddMediaItems";

        var batchCreateResponse = await _httpClientWrapper.PostAsync<
            BatchCreateMediaItemsRequest,
            BatchCreateMediaItemsResponse>(requestUri, batchCreateRequest);

        return batchCreateResponse;
    }

    // https://developers.google.com/photos/library/reference/rest/v1/albums/create
    public async Task<Result<Album>> CreateAlbumAsync(string albumName)
    {
        var albumRequest = new CreateAlbumRequest()
        {
            Album = new Album()
            {
                Title = albumName
            }
        };
        
        string requestUri = "https://photoslibrary.googleapis.com/v1/album";

        var createAlbumResponse = await _httpClientWrapper.PostAsync<CreateAlbumRequest, Album>(
            requestUri,
            albumRequest);

        return createAlbumResponse;
    }

    // https://developers.google.com/photos/library/reference/rest/v1/albums/get
    public async Task<Result<Album>> GetAlbumFromIdAsync(string albumId)
    {
        string requestUri = $"https://photoslibrary.googleapis.com/v1/albums/{albumId}";

        var getAlbumResponse = await _httpClientWrapper.GetAsync<Album>(requestUri);

        return getAlbumResponse;
    }

    // https://developers.google.com/photos/library/reference/rest/v1/albums/list
    public async Task<Result<Album>> GetAlbumFromTitleAsync(string albumName)
    {
        string requestUri = $"https://photoslibrary.googleapis.com/v1/albums/";


        var getAlbumsResponse = await _httpClientWrapper.GetAsync<ListAlbumsResponse>(requestUri);

        if (!getAlbumsResponse.Succeeded)
        {
            return Result<Album>.Failure(getAlbumsResponse.ErrorMessage ?? "Failed to get albums");
        }

        var album = getAlbumsResponse.Value?.Albums.FirstOrDefault(a => a.Title == albumName);
        if (album == null)
        {
            return Result<Album>.Failure($"Album with name '{albumName}' not found");
        }

        return Result<Album>.Success(album);
    }
}
