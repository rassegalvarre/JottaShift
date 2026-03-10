using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace JottaShift.Core.Jottacloud;

public class JottacloudRepository(
    ILogger<JottacloudRepository> _logger,
    IFileStorage _fileStorage,
    IJottacloudClient _client,
    JottacloudSettings _settings) : IJottacloudRepository
{
    private CultureInfo _culture { get; set; } = CultureInfo.CurrentCulture;

    public void SetCulture(CultureInfo culture)
    {
        _culture = culture;
    }

    public async Task<IEnumerable<PhotoDto>> GetAlbumPhotos(string albumId)
    {
        var albumResult = await _client.GetAlbumAsync(albumId);
        if (!albumResult.Success || albumResult.Value == null)
        {
            _logger.LogWarning("Fetching album with id {AlbumId} failed", albumId);
            return [];
        }

        List<PhotoDto> photoDtos = [];
        foreach (var photo in albumResult.Value.Photos)
        {
            var photoDto = new PhotoDto(photo);
            string predicatedSearchFolder = JottacloudAdapter.PhotoStorageStructuredDirectoryPath(
                photoDto.CapturedDate,
                _settings.PhotoStoragePath,
                _culture);
            var localFilePathResult = _fileStorage.SearchFileByExactName(predicatedSearchFolder, photo.Filename);
            
            if (!localFilePathResult.Success)
            {
                _logger.LogWarning("Photo named {PhotoName} was not found in the expected local directory. Searched in: {SearchDirectory}",
                    photoDto.ImageName,
                    predicatedSearchFolder);
            }

            photoDto.LocalFilePath = localFilePathResult.Value;
            photoDtos.Add(photoDto);
        }

        return photoDtos;
    }
}
