using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.Jottacloud;

public class JottacloudRepository(
    ILogger<JottacloudRepository> _logger,
    IFileStorage _fileStorage,
    JottacloudSettings _settings) : IJottacloudRepository
{
    public async Task<IEnumerable<string>> GetImagesInAlbumAsync(string albumId)
    {
        await Task.CompletedTask;
        return [];
    }

    public async Task<string> GetImageFilePathFromFileName(string imageFileName)
    {
        var imageDate = _fileStorage.GetImageDate(imageFileName);
        if (imageDate == default)
        {
            return string.Empty;
        }

        // TODO: Need to handle "01 - <monthname>". Move from Orchestrator
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
