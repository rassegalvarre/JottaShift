using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos.PhotosLibraryV1;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosLibraryHttpClient : IGooglePhotosLibraryHttpClient
{
    private readonly IFileStorageService _fileStorage;
    private readonly IHttpClientWrapper _http;
    private readonly IUserCredentialManager _userCredentialManager;
    private readonly ILogger<GooglePhotosLibraryHttpClient> _logger;

    public GooglePhotosLibraryHttpClient(
        IFileStorageService fileStorage,
        IHttpClientWrapper http,
        IUserCredentialManager userCredentialManager,
        ILogger<GooglePhotosLibraryHttpClient> logger)
    {
        _fileStorage = fileStorage;
        _http = http;
        _http.BaseAddress = _googlePhotosLibraryApiUri;
        _userCredentialManager = userCredentialManager;
        _logger = logger;
    }

    private readonly Uri _googlePhotosLibraryApiUri = new("https://photoslibrary.googleapis.com/v1/");
    private const int _maxItemsPerCall = 50;

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
        string endpoint,
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

        // Add a "./" prefix to ensure the endpoint is treated as relative to the base address
        // Avoids some issues that can occur with the gRPC transcoded path. https://google.aip.dev/127
        var request = new HttpRequestMessage(httpMethod, $"./{endpoint}")
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

        var content = SerializeToByteArrayContent(fileContentResult.Value);
        var additionalHeaders = new Dictionary<string, string>()
        {
            { "X-Goog-Upload-File-Name", fileNameResult.Value },
            { "X-Goog-Upload-Protocol", "raw" }
        };

        return await SendWithBearerTokenAsync<string>("uploads", HttpMethod.Post,
            content, additionalHeaders);
    }

    public async Task<Result> BatchAddMediaItemsAsync(string albumId, IEnumerable<string> mediaIds)
    {
        if (mediaIds.Count() > _maxItemsPerCall)
        {
            return Result.Failure($"Cannot add more than {_maxItemsPerCall} media items in a single call.");
        }

        var batchAddRequest = new BatchAddMediaItemsRequest
        {
            MediaItemIds = [.. mediaIds]
        };
        var content = SerializeToStringContent(batchAddRequest);
        return await SendWithBearerTokenAsync<string>($"albums/{albumId}:batchAddMediaItems",
            HttpMethod.Post, content);
    }

    public Task<Result> BatchAddMediaItemsAsync(string albumId, BatchCreateMediaItemsResponse createMediaItemsResponse)
    {
        var mediaItemIds = createMediaItemsResponse.NewMediaItemResults?
           .Where(r => r.MediaItem?.Id is not null)
           .Select(r => r.MediaItem!.Id!)
           .ToList() ?? []; // TODO: Refactor to avoid nullability issues

        return BatchAddMediaItemsAsync(albumId, mediaItemIds);
    }

    public async Task<Result<BatchCreateMediaItemsResponse>> BatchCreateMediaItemsAsync(string albumId, IEnumerable<string> uploadTokens)
    {
        if (uploadTokens.Count() > _maxItemsPerCall)
        {
            return Result<BatchCreateMediaItemsResponse>.Failure($"Cannot add more than {_maxItemsPerCall} media items in a single call.");
        }

        var batchCreateRequest = new BatchCreateMediaItemsRequest
        {
            AlbumId = albumId,
            NewMediaItems = [.. uploadTokens.Select(token => new NewMediaItem
            {
                SimpleMediaItem = new SimpleMediaItem { UploadToken = token }
            })],
            AlbumPosition = new AlbumPosition()
            {
                Position = PositionType.POSITION_TYPE_UNSPECIFIED
            }
        };

        var content = SerializeToStringContent(batchCreateRequest);

        return await SendWithBearerTokenAsync<BatchCreateMediaItemsResponse>(
            "mediaItems:batchCreate",
            HttpMethod.Post, content);
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

        var content = SerializeToStringContent(albumRequest);

        return SendWithBearerTokenAsync<Album>(
            "album", HttpMethod.Post, content);
    }
 
    public Task<Result<Album>> GetAlbumAsync(string albumId)
    {
        return SendWithBearerTokenAsync<Album>($"albums/{albumId}", HttpMethod.Get);
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
        return SendWithBearerTokenAsync<ListAlbumsResponse>("albums", HttpMethod.Get);
    }
    #endregion  
}
