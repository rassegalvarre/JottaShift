using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Jottacloud.Models.Domain;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.Jottacloud;

public class JottacloudClient : IJottacloudClient
{
    private readonly ILogger<JottacloudClient> _logger;
    private readonly IHttpClientWrapper _http;

    public JottacloudClient(IHttpClientWrapper http, ILogger<JottacloudClient> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.jottacloud.com/");
        _logger = logger;
    }

    public async Task<Result<Album>> GetAlbumAsync(string albumId, int limit = 100)
    {
        var requestUri = $"/photos/v1/public/{albumId}/?order=ASC&limit={limit}";

        var result = _http.GetAsync<Album>(requestUri).Result;

        if (result.Success && result.Content != null)
        {
            return Result<Album>.Success(result.Content);
        }
        else
        {
            _logger.LogError("Failed to get album with id {AlbumId}", albumId);
            return Result<Album>.Failure("Failed to get album");
        }
    }
}