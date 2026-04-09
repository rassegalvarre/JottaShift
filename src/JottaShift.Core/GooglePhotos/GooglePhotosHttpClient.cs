using Google.Apis.PhotosLibrary.v1.Data;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos.Models.Api;
using JottaShift.Core.GooglePhotos.Models.Domain;
using JottaShift.Core.GooglePhotos.Models.PhotosLibraryApi;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosHttpClient(
    IFileStorageService _fileStorage,
    IHttpClientWrapper _http,
    IUserCredentialManager _userCredentialManager,
    ILogger<GooglePhotosHttpClient> _logger) : IGooglePhotosHttpClient, IGooglePhotosLibraryFacade
{
    #region Private helper methods
    private static StringContent SerializeToStringContent<T>(T obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    private static ByteArrayContent SerializeToByteArrayContent(byte[] bytes)
    {
        return new ByteArrayContent(bytes)
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/octet-stream") }
        };
    }

    private async Task<Result<TResponse>> SendWithBearerTokenAsync<TResponse>(
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

        var request = new HttpRequestMessage(httpMethod, requestUri)
        {
            Content = requestContent,
        };

        foreach (var header in additionalHeaders ?? [])
        {
            request.Headers.Add(header.Key, header.Value);
        }        

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenResult.Value);
        var response = await _http.SendAsync<TResponse>(request);
        return response.ToResult();
    }
    #endregion

    #region Public API methods
    public async Task<Result<string>> UploadMediaAsync(string fileFullPah)
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

        const string requestUri = "https://photoslibrary.googleapis.com/v1/uploads";
        var content = SerializeToByteArrayContent(fileContentResult.Value);
        var additionalHeaders = new Dictionary<string, string>()
        {
            { "X-Goog-Upload-File-Name", fileNameResult.Value },
            { "X-Goog-Upload-Protocol", "raw" }
        };

        var result = await SendWithBearerTokenAsync<string>(
            requestUri,
            HttpMethod.Post,
            content,            
            additionalHeaders);

        return result;
    }



    public async Task<Result<BatchCreateMediaItemsResponse>> BatchAddMediaItemsAsync(string albumId, IEnumerable<string> uploadTokens)
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
        var content = SerializeToStringContent(batchCreateRequest);

        var batchCreateResponse = await SendWithBearerTokenAsync<BatchCreateMediaItemsResponse>(
            requestUri,
            HttpMethod.Post,
            content);

        return batchCreateResponse;
    }

    /// <remarks>
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/create">Method: albums.create</see>
    /// </remarks>
    public async Task<Result<GooglePhotosAlbum>> CreateAlbumAsync(string albumName)
    {
        var albumRequest = new AlbumsCreateRequest()
        {
            Album = new GooglePhotosAlbum()
            {
                Title = albumName
            }
        };

        string requestUri = "https://photoslibrary.googleapis.com/v1/album";
        var content = SerializeToStringContent(albumRequest);

        var createAlbumResponse = await SendWithBearerTokenAsync<GooglePhotosAlbum>(
            requestUri,
            HttpMethod.Post,
            content);

        return createAlbumResponse;
    }

    /// <remarks>
    /// <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/get">Method: albums.get</see>
    /// </remarks>
    public async Task<Result<Album>> GetAlbumFromIdAsync(string albumId)
    {
        string requestUri = $"https://photoslibrary.googleapis.com/v1/albums/{albumId}";

        var getAlbumResponse = await SendWithBearerTokenAsync<Album>(requestUri, HttpMethod.Get);

        return getAlbumResponse;
    }

    // TODO: Extract List to own method
    /// <remarks>
    /// Uses <see href="https://developers.google.com/photos/library/reference/rest/v1/albums/list">Method: albums.list</see>
    /// </remarks>
    public async Task<Result<Album>> GetAlbumFromTitleAsync(string albumName)
    {
        string requestUri = $"https://photoslibrary.googleapis.com/v1/albums/";

        var getAlbumsResponse = await SendWithBearerTokenAsync<AlbumsResponse>(requestUri, HttpMethod.Get);

        if (!getAlbumsResponse.Succeeded || getAlbumsResponse.Value is null)
        {
            return Result<Album>.Failure(getAlbumsResponse.ErrorMessage ?? "Failed to get albums");
        }

        var album = getAlbumsResponse.Value.Albums.FirstOrDefault(a => a.Title == albumName);
        if (album == null)
        {
            return Result<Album>.Failure($"Album with name '{albumName}' not found");
        }

        return Result<Album>.Success(new Album()
        {
            Title = album.Title
        });
    }
    #endregion

    #region Obsolete API methods
    [Obsolete("Use BatchAddMediaItemsAsync instead")]
    public Task<Result<BatchCreateMediaItemsResponse>> AddImagesToAlbum(string albumId, IEnumerable<string> uploadTokens)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Use CreateAlbumAsync instead")]
    Task<Result<Album>> IGooglePhotosLibraryFacade.CreateAlbumAsync(string albumName)
    {
        throw new NotImplementedException();
    }
    #endregion
}
