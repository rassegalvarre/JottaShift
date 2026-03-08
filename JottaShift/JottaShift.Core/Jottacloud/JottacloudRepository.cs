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

    public async Task<GetAlbumResponse> GetAlbum(string albumId)
    {
        string requestUri = $"/photos/v1/public/{albumId}/?order=ASC&limit={DefaultLimit}";
        
        // TODO: Inject
        var httpClient = new HttpClient()
        {
            BaseAddress = BaseApiUrl
        };

        var response = await httpClient.GetAsync(requestUri);
        if (response.StatusCode is not System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Requesting album with id {AlbumId} resulted in status {HttpStatus}",
                albumId, response.StatusCode);
            response.EnsureSuccessStatusCode();
        }

        var responseContentStream = await response.Content.ReadAsStreamAsync();
        var album = await JsonSerializer.DeserializeAsync<GetAlbumResponse>(responseContentStream);
        if (album == null)
        {
            _logger.LogError("Could not deserialize response to the expected schema");
            throw new NotSupportedException("Unexpected response content");
        }

        //List<string> fileNames = [];

        //foreach (var item in album.Photos)
        //{
        //    var imageDate = item.CapturedDate; // Use this to determine image location on disk.
        //    fileNames.Add(item.Filename);
        //}

        return album;
    }
    
    //public async Task<IEnumerable<string>> GetLocalPathForImagesInAlbumAsync(string albumId)
    //{
    //    List<string> localFilePaths = [];

    //    var images = await GetAlbum(albumId);

    //    foreach (var image in images)
    //    {
    //        string localPath = await GetImageFilePathFromFileName(image);
    //        if (string.IsNullOrEmpty(localPath))
    //        {
    //            _logger.LogWarning("Local path for image {ImageName} from album {AlbumId} could not be determined",
    //                image, albumId);
    //            continue;
    //        }

    //        localFilePaths.Add(localPath);
    //    }

    //    return localFilePaths;
    //}

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

    public async Task<IEnumerable<PhotoDto>> GetAlbumImages(string albumId)
    {
        var album = await GetAlbum(albumId);
        List<PhotoDto> photoDtos = [];

        foreach (var photo in album.Photos)
        {
            string searchFolder = string.Empty; // TOOD: Calculate
            var localFilePath = _fileStorage.SearchFileByExactName(searchFolder, photo.Filename);
            var photoDto = new PhotoDto(photo, localFilePath);
            photoDtos.Add(photoDto);
        }

        return photoDtos;
    }

    public string PredictPhotoDirectory(Photo photo)
    {
        return string.Empty;
    }
}
