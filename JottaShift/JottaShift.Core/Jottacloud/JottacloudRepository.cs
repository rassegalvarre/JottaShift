using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.Jottacloud;

public class JottacloudRepository(
    ILogger<JottacloudRepository> _logger,
    IFileStorage _fileStorage) : IJottacloudRepository
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



        await Task.CompletedTask;
        return string.Empty;
    }
}
