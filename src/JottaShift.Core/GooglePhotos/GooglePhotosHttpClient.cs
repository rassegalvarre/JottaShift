using Google.Apis.PhotosLibrary.v1.Data;
using JottaShift.Core.FileStorage;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosHttpClient(
    IFileStorageService _fileStorage,
    IHttpClientWrapper _http,
    IUserCredentialManager _userCredentialManager,
    ILogger<GooglePhotosHttpClient> _logger) : IGooglePhotosHttpClient, IGooglePhotosLibraryFacade
{
    public async Task<Result<TResponse>> SendWithBearerTokenAsync<TRequest, TResponse>(
        string requestUri,
        HttpMethod httpMethod,
        HttpContent? requestContent = null,
        Dictionary<string, string>? additionalHeaders = null)
    {
        var accessTokenResult = await _userCredentialManager.GetAccessTokenAsync();
        if (!accessTokenResult.Succeeded || accessTokenResult.Value == null)
        {
            _logger.LogError("Failed to get access token: {ErrorMessage}", accessTokenResult.ErrorMessage);
            return Result<TResponse>.Failure("Failed to get access token.");
        }

        var request = new HttpRequestMessage(httpMethod, requestUri);
        if (requestContent != null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(requestContent));
        }

        if (additionalHeaders != null)
        {
            foreach (var header in additionalHeaders)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResult.Value);
        var response = await _http.SendAsync<TResponse>(request);
        return response.ToResult();
    }

    // Replace with?
    // https://developers.google.com/photos/library/reference/rest/v1/mediaItems/batchCreate
    public async Task<Result<string>> UploadPhotoAsync(string fileFullPah)
    {
        var fileContentResult = await _fileStorage.GetFileBytesAsync(fileFullPah);
        var fileNameResult = _fileStorage.GetFileName(fileFullPah);

        if (!fileContentResult.Succeeded || fileContentResult.Value is null)
        {
            _logger.LogError("Failed to get file content path: {FilePath}. Error: {ContentError}",
                fileFullPah, fileContentResult.ErrorMessage);
            return Result<string>.Failure("Failed to get file content");
        }
        if (!fileNameResult.Succeeded || fileNameResult.Value is null)
        {
            _logger.LogError("Failed to get file name from path: {FilePath}. Error: {NameError}",
                fileFullPah, fileNameResult.ErrorMessage);
            return Result<string>.Failure("Failed to get file name");
        }

        var accessTokenResult = await _userCredentialManager.GetAccessTokenAsync();
        if (!accessTokenResult.Succeeded || accessTokenResult.Value is null)
        {
            _logger.LogError("Failed to get access token: {ErrorMessage}", accessTokenResult.ErrorMessage);
            return Result<string>.Failure("Failed to get access token.");
        }

        const string requestUri = "https://photoslibrary.googleapis.com/v1/uploads";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new ByteArrayContent(fileContentResult.Value),
        };

        // Required headers (see Google docs)
        request.Headers.Add("Authorization", $"Bearer {accessTokenResult.Value}");
        request.Headers.Add("X-Goog-Upload-File-Name", fileNameResult.Value);
        request.Headers.Add("X-Goog-Upload-Protocol", "raw");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var result = await _http.SendAsync<string>(request);
        return result.ToResult();
    }

    
    
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

        var batchCreateResponse = await _http.PostAsync<
            BatchCreateMediaItemsRequest,
            BatchCreateMediaItemsResponse>(requestUri, batchCreateRequest);

        return batchCreateResponse.ToResult();
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

        var createAlbumResponse = await _http.PostAsync<CreateAlbumRequest, Album>(
            requestUri,
            albumRequest);

        return createAlbumResponse.ToResult();
    }

    // https://developers.google.com/photos/library/reference/rest/v1/albums/get
    public async Task<Result<Album>> GetAlbumFromIdAsync(string albumId)
    {
        string requestUri = $"https://photoslibrary.googleapis.com/v1/albums/{albumId}";

        var getAlbumResponse = await _http.GetAsync<Album>(requestUri);

        return getAlbumResponse.ToResult();
    }

    // https://developers.google.com/photos/library/reference/rest/v1/albums/list
    public async Task<Result<Album>> GetAlbumFromTitleAsync(string albumName)
    {
        string requestUri = $"https://photoslibrary.googleapis.com/v1/albums/";


        var getAlbumsResponse = await _http.GetAsync<ListAlbumsResponse>(requestUri);

        if (!getAlbumsResponse.Success)
        {
            return Result<Album>.Failure(getAlbumsResponse.ErrorMessage ?? "Failed to get albums");
        }

        var album = getAlbumsResponse.Content?.Albums.FirstOrDefault(a => a.Title == albumName);
        if (album == null)
        {
            return Result<Album>.Failure($"Album with name '{albumName}' not found");
        }

        return Result<Album>.Success(album);
    }
}
