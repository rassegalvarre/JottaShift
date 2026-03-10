using Google.Apis.Auth.OAuth2;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosHttpClient : IGooglePhotosHttpClient
{
    private readonly ILogger<GooglePhotosHttpClient> _logger;
    private readonly IHttpClientWrapper _http;

    public GooglePhotosHttpClient(IHttpClientWrapper http, ILogger<GooglePhotosHttpClient> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://photoslibrary.googleapis.com/v1/");
        _logger = logger;
    }

    public async Task<Result<string>> UploadPhoto(UserCredential userCredential, string fileName, byte[] fileData)
    {
        const string uploadUrl = "uploads";

        var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
        {
            Content = new ByteArrayContent(fileData),
        };

        // Required headers (see Google docs)
        request.Headers.Add("Authorization", $"Bearer {userCredential.Token.AccessToken}");
        request.Headers.Add("X-Goog-Upload-File-Name", fileName);
        request.Headers.Add("X-Goog-Upload-Protocol", "raw");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var result = await _http.SendAsync<string>(request);
        return result.ToResult();
    }
}
