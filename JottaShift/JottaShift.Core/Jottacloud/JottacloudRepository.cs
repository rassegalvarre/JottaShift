using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace JottaShift.Core.Jottacloud;

public class JottacloudRepository(
    ILogger<JottacloudRepository> _logger,
    IFileStorage _fileStorage,
    JottacloudClient _client,
    JottacloudSettings _settings) : IJottacloudRepository
{
    private readonly Uri BaseApiUrl = new Uri("https://api.jottacloud.com/");

    private const int DefaultLimit = 100;

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
        var album = await _client.GetAlbumAsync(albumId);
        List<PhotoDto> photoDtos = [];

        foreach (var photo in album.Photos)
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

    public string PhotoStorageDirectoryPath(DateTimeOffset photoCaputedDato)
    {
        string year = photoCaputedDato.Year.ToString();
        string monthDirectoryName = GetMonthDirectoryName(photoCaputedDato.Month);

        string predictedDirectory = Path.Combine(
            _settings.PhotoStoragePath,
            year,
            monthDirectoryName);

        return Path.Combine(_settings.PhotoStoragePath, predictedDirectory);
    }

    public string GetMonthDirectoryName(int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12");

        int monthIndex = month - 1;

        string monthName = _culture.DateTimeFormat.MonthNames[monthIndex];
        string capitalizedMonthName = char.ToUpper(monthName[0]) + monthName[1..];

        return $"{monthIndex + 1:D2} {capitalizedMonthName}";
    }
}
