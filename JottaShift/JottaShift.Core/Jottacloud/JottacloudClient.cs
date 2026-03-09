using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JottaShift.Core.Jottacloud;

public class JottacloudClient
{
    private readonly ILogger<JottacloudClient> _logger;
    private readonly HttpClient _http;

    public JottacloudClient(HttpClient http, ILogger<JottacloudClient> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.jottacloud.com/");
        _logger = logger;
    }

    public async Task<GetAlbumResponse> GetAlbumAsync(string albumId, int limit = 100)
    {
        string requestUri = $"/photos/v1/public/{albumId}/?order=ASC&limit={limit}";

        HttpResponseMessage response;
        Album? album;

        try
        {
            response = await _http.GetAsync(requestUri);

            if (response.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                _logger.LogError("Requesting album with id {AlbumId} resulted in status {HttpStatus}",
                    albumId, response.StatusCode);

                return new GetAlbumResponse(false);
            }
            var responseContentStream = await response.Content.ReadAsStreamAsync();
            album = await JsonSerializer.DeserializeAsync<Album?>(responseContentStream);

            if (album == null)
            {
                _logger.LogError("Could not deserialize response to the expected schema");
                return new GetAlbumResponse(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while requesting album with id {AlbumId}", albumId);
            return new GetAlbumResponse(false);
        }

        return new GetAlbumResponse(true) with
        {
            Album = album,
        };
    }
}