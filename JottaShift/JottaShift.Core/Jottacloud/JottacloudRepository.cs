using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.Jottacloud;

public class JottacloudRepository(
    ILogger<JottacloudRepository> _logger,
    IFileStorage fileStorage) : IJottacloudRepository
{
    public async Task<IEnumerable<string>> GetImagesInAlbumAsync(string albumId)
    {
        await Task.CompletedTask;
        return [];
    }

    public async Task<string> GetImageFilePathFromFileName(string imageFileName)
    {
        await Task.CompletedTask;
        return string.Empty;
    }
}
