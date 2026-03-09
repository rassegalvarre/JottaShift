using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace JottaShift.Core.Jottacloud;

public class JottacloudRepository(
    ILogger<JottacloudRepository> _logger,
    IFileStorage _fileStorage,
    JottacloudClient _client,
    JottacloudSettings _settings) : IJottacloudRepository
{
    private CultureInfo _culture { get; set; } = CultureInfo.CurrentCulture;

    public void SetCulture(CultureInfo culture)
    {
        _culture = culture;
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
            _settings.PhotoStoragePath,
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

    public async Task<IEnumerable<PhotoDto>> GetAlbumImages(string albumId)
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
            string predicatedSearchFolder = PhotoStorageDirectoryPath(photoDto.CapturedDate);
            var localFilePath = _fileStorage.SearchFileByExactName(predicatedSearchFolder, photo.Filename);
            
            if (string.IsNullOrEmpty(localFilePath))
            {
                _logger.LogWarning("Photo named {PhotoName} was not found in the expected local directory. Searched in: {SearchDirectory}",
                    photoDto.ImageName,
                    predicatedSearchFolder);
                continue;
            }

            photoDto.LocalFilePath = localFilePath;
            photoDtos.Add(photoDto);
        }

        return photoDtos;
    }

    [Obsolete("Moved to Adapter")]
    public string PhotoStorageDirectoryPath(DateTimeOffset photoCaputedDato)
    {
        string year = photoCaputedDato.Year.ToString();
        string monthDirectoryName = JottacloudAdapter.GetMonthDirectoryName(photoCaputedDato.Month, _culture);

        string predictedDirectory = Path.Combine(
            _settings.PhotoStoragePath,
            year,
            monthDirectoryName);

        return Path.Combine(_settings.PhotoStoragePath, predictedDirectory);
    }   
}
