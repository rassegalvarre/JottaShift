using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos.PhotosLibraryV1;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosHttpClient(
    IFileStorageService _fileStorage,
    IHttpClientWrapper _http,
    IUserCredentialManager _userCredentialManager,
    ILogger<GooglePhotosHttpClient> _logger) : IGooglePhotosHttpClient
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

    #region Google Photos Library API methods
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

        return await SendWithBearerTokenAsync<string>(
            requestUri,
            HttpMethod.Post,
            content,            
            additionalHeaders);
    }

    public Task<Result<BatchCreateMediaItemsResponse>> BatchAddMediaItemsAsync(string albumId, IEnumerable<string> uploadTokens)
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

        return SendWithBearerTokenAsync<BatchCreateMediaItemsResponse>(
            requestUri,
            HttpMethod.Post,
            content);
    }

    public Task<Result<Album>> CreateAlbumAsync(string albumTitle)
    {
        var albumRequest = new CreateAlbumRequest()
        {
            Album = new Album()
            {
                Title = albumTitle
            }
        };

        string requestUri = "https://photoslibrary.googleapis.com/v1/album";
        var content = SerializeToStringContent(albumRequest);

        return SendWithBearerTokenAsync<Album>(
            requestUri,
            HttpMethod.Post,
            content);
    }
 
    public Task<Result<Album>> GetAlbumAsync(string albumId)
    {
        string requestUri = $"https://photoslibrary.googleapis.com/v1/albums/{albumId}";

        return SendWithBearerTokenAsync<Album>(requestUri, HttpMethod.Get);
    }
   
    public async Task<Result<Album>> GetAlbumFromTitleAsync(string albumTitle)
    {       
        var listAlbumsResponse = await ListAlbumsAsync();

        if (!listAlbumsResponse.Succeeded || listAlbumsResponse.Value is null)
        {
            return listAlbumsResponse.ForwardFailure<Album>();
        }

        var album = listAlbumsResponse.Value.Albums?.FirstOrDefault(a => a.Title == albumTitle);
        if (album == null)
        {
            return Result<Album>.Failure($"Album with title '{albumTitle}' not found");
        }

        return Result<Album>.Success(album);
    }

    public Task<Result<ListAlbumsResponse>> ListAlbumsAsync()
    {
        string requestUri = $"https://photoslibrary.googleapis.com/v1/albums/";

        return SendWithBearerTokenAsync<ListAlbumsResponse>(requestUri, HttpMethod.Get);
    }
    #endregion  
}
