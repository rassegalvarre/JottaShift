using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.Jottacloud;

public class JottacloudClient
{
    private readonly ILogger<JottacloudClient> _logger;
    private readonly IHttpClientWrapper _http;

    public JottacloudClient(IHttpClientWrapper http, ILogger<JottacloudClient> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.jottacloud.com/");
        _logger = logger;
    }

    public async Task<Album> GetAlbumAsync(string albumId, int limit = 100)
    {
        var requestUri = new Uri($"/photos/v1/public/{albumId}/?order=ASC&limit={limit}");

        var result = _http.GetAsync<Album>(requestUri).Result;

        if (result.Success && result.Data != null)
        {
            return result.Data;
        }
        else
        {
            _logger.LogError("Failed to get album with id {AlbumId}", albumId);
            throw new Exception("Failed to get album");
        }
    }
}