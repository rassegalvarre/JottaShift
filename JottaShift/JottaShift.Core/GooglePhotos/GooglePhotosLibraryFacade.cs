using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosLibraryFacade : IGooglePhotosLibraryFacade
{
    private readonly ILogger<GooglePhotosLibraryFacade> _logger;
    private readonly Func<Task<UserCredential>> _getCredentialAsync;
    private readonly Lazy<Task<PhotosLibraryService>> _photoLibraryService;

    public GooglePhotosLibraryFacade(
        ILogger<GooglePhotosLibraryFacade> logger,
        Func<Task<UserCredential>> getCredentialAsync)
    {
        _logger = logger;
        _getCredentialAsync = getCredentialAsync;
        _photoLibraryService = new Lazy<Task<PhotosLibraryService>>(InitializeServiceAsync());
    }

    private async Task<PhotosLibraryService> InitializeServiceAsync()
    {
        _logger.LogInformation("Initializing PhotosLibraryService");

        var credential = await _getCredentialAsync();

        var service = new PhotosLibraryService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "JottaShift"
        });

        _logger.LogInformation("PhotosLibraryService initialized successfully");
        return service;
    }

    private async Task<PhotosLibraryService> GetServiceAsync()
    {
        _logger.LogDebug("Getting PhotosLibraryService");
        return await _photoLibraryService.Value;
    }

    public async Task<Result<Album>> GetAlbumAsync(string albumName)
    {
        try
        {
            var photosLibraryService = await GetServiceAsync();

            var albumListResponse = await photosLibraryService.Albums.List().ExecuteAsync();
            var album = albumListResponse.Albums?.FirstOrDefault(a => a.Title == albumName);

            return album != null 
                ? Result<Album>.Success(album)
                : Result<Album>.Failure("Album not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get album {AlbumName}", albumName);
            return Result<Album>.Failure("An exception occurred");
        }
    }
}
