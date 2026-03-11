using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using JottaShift.Core.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosLibraryFacade : IGooglePhotosLibraryFacade
{
    private readonly GooglePhotosLibraryApiCredentials _apiCredentials;
    private readonly ILogger<GooglePhotosLibraryFacade> _logger;
    private readonly Lazy<Task<PhotosLibraryService>> _photoLibraryService;
    private UserCredential? _userCredential;

    private readonly string[] _scopes = [
        PhotosLibraryService.Scope.PhotoslibraryAppendonly,
        PhotosLibraryService.Scope.PhotoslibraryReadonlyAppcreateddata
    ];

    public GooglePhotosLibraryFacade(
        GooglePhotosLibraryApiCredentials apiCredentials,
        ILogger<GooglePhotosLibraryFacade> logger)
    {
        if (EnvironmentVariableManager.GooglePhotosLibraryApiProjectId == null ||
            EnvironmentVariableManager.GooglePhotosLibraryApiClientId == null ||
            EnvironmentVariableManager.GooglePhotosLibraryApiClientSecret == null)
        {
            throw new InvalidOperationException("Required environment variables for Google Photos API are not set.");
        }

        _apiCredentials = apiCredentials;
        _apiCredentials.installed.project_id = EnvironmentVariableManager.GooglePhotosLibraryApiProjectId;
        _apiCredentials.installed.client_id = EnvironmentVariableManager.GooglePhotosLibraryApiClientId;
        _apiCredentials.installed.client_secret = EnvironmentVariableManager.GooglePhotosLibraryApiClientSecret;

        _logger = logger;
        _photoLibraryService = new Lazy<Task<PhotosLibraryService>>(InitializeServiceAsync());
    }

    private async Task<Result<UserCredential>> GetCredentialAsync()
    {
        if (_userCredential != null)
        {
            if (_userCredential.Token.IsStale)
            {
                var refreshed = await _userCredential.RefreshTokenAsync(CancellationToken.None);
                if (!refreshed)
                {
                    return Result<UserCredential>.Failure("Could not refresh user credentials");
                }
            }
            return Result<UserCredential>.Success(_userCredential);
        }

        var json = JsonSerializer.Serialize(_apiCredentials);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        var secretsResult = await GoogleClientSecrets.FromStreamAsync(stream);

        // Token will be stored in the token.json folder
        var credPath = "token.json";
        UserCredential newCredentials;
        try
        {
            newCredentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secretsResult.Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));
        }
        catch (Exception ex)
        {
            return Result<UserCredential>.Failure($"Failed to authorize user credentials: {ex.Message}");
        }

        _userCredential = newCredentials;
        return Result<UserCredential>.Success(_userCredential);
    }

    private async Task<PhotosLibraryService> InitializeServiceAsync()
    {
        _logger.LogInformation("Initializing PhotosLibraryService");

        var credential = await GetCredentialAsync();
        if (!credential.Succeeded)
        {
            _logger.LogError("Failed to obtain user credentials: {ErrorMessage}", credential.ErrorMessage);
            throw new Exception();
        }

        var service = new PhotosLibraryService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential.Value,
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
