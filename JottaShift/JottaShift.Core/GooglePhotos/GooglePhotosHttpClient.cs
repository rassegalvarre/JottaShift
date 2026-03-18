using Google.Apis.Auth.OAuth2;
using JottaShift.Core.FileStorage;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosHttpClient : IGooglePhotosHttpClient
{
    private readonly IFileStorageService _fileStorage;
    private readonly IHttpClientWrapper _http;
    private readonly IUserCredentialManager _userCredentialManager;
    private readonly ILogger<GooglePhotosHttpClient> _logger;

    public GooglePhotosHttpClient(
        IFileStorageService fileStorage,
        IHttpClientWrapper http,
        IUserCredentialManager userCredentialManager,
        ILogger<GooglePhotosHttpClient> logger)
    {
        _fileStorage = fileStorage;
        _http = http;
        // _http.BaseAddress = new Uri("https://photoslibrary.googleapis.com/v1/");

        _userCredentialManager = userCredentialManager;
        _logger = logger;
    }

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

        //const string uploadUrl = "uploads";
        //var requesturi = new Uri(_http.BaseAddress!, uploadUrl);

        const string uploadUrl = "https://photoslibrary.googleapis.com/v1/uploads";

        var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
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
}
