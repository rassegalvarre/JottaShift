using Google.Apis.Auth.OAuth2;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosHttpClient : IGooglePhotosHttpClient
{
    private readonly IHttpClientWrapper _http;
    private readonly IUserCredentialManager _userCredentialManager;
    private readonly ILogger<GooglePhotosHttpClient> _logger;

    public GooglePhotosHttpClient(
        IHttpClientWrapper http,
        IUserCredentialManager userCredentialManager,
        ILogger<GooglePhotosHttpClient> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://photoslibrary.googleapis.com/v1/");
        _userCredentialManager = userCredentialManager;
        _logger = logger;
    }

    public async Task<Result<string>> UploadPhoto(string fileName, byte[] fileData)
    {
        var credentialResult = await _userCredentialManager.GetCredentialAsync();
        if (!credentialResult.Succeeded || credentialResult.Value is null)
        {
            _logger.LogError("Failed to get user credential: {ErrorMessage}", credentialResult.ErrorMessage);
            return Result<string>.Failure("Failed to get user credential.");
        }

        const string uploadUrl = "uploads";

        var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
        {
            Content = new ByteArrayContent(fileData),
        };

        // Required headers (see Google docs)
        request.Headers.Add("Authorization", $"Bearer {credentialResult.Value.Token.AccessToken}");
        request.Headers.Add("X-Goog-Upload-File-Name", fileName);
        request.Headers.Add("X-Goog-Upload-Protocol", "raw");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var result = await _http.SendAsync<string>(request);
        return result.ToResult();
    }
}
