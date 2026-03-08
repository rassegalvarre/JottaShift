using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JottaShift.Core.Jottacloud;

public class JottacloudRepository(
    ILogger<JottacloudRepository> _logger,
    IFileStorage _fileStorage,
    JottacloudSettings _settings) : IJottacloudRepository
{
    private readonly Uri BaseApiUrl = new Uri("https://api.jottacloud.com/");

    private const int DefaultLimit = 100;

    public async Task<IEnumerable<string>> GetImagesInAlbumAsync(string albumId)
    {
        string requestUri = $"/photos/v1/public/{albumId}/?order=ASC&limit={DefaultLimit}";
        
        // TODO: Inject
        var httpClient = new HttpClient()
        {
            BaseAddress = BaseApiUrl
        };

        var response = await httpClient.GetAsync(requestUri);

        response.EnsureSuccessStatusCode();

        var contentJson = await response.Content.ReadAsStreamAsync();

        var album = await JsonSerializer.DeserializeAsync<GetAlbumResponse>(contentJson);
        if (album == null)
        {
            _logger.LogError("Response content is empty");
            return Enumerable.Empty<string>();
        }

        List<string> fileNames = [];

        foreach (var item in album.Photos)
        {
            fileNames.Add(item.Filename);
        }

        return fileNames;
    }

    public async Task<string> GetImageFilePathFromFileName(string imageFileName)
    {
        var imageDate = _fileStorage.GetImageDate(imageFileName);
        if (imageDate == default)
        {
            return string.Empty;
        }

        // TODO: Need to handle "01 - <monthname>". Move that logic from Orchestrator
        string searchDirectory = Path.Combine(
            _settings.ImageStoragePath,
            imageDate.Year.ToString(),
            imageDate.Month.ToString());

        var imageFullPath = _fileStorage.SearchFileByExactName(searchDirectory, imageFileName);
        if (string.IsNullOrEmpty(imageFullPath))
        {
            _logger.LogWarning("Could not find path for image {ImageName}", imageFileName);
            return string.Empty;
        }

        return imageFullPath;
    }
}
