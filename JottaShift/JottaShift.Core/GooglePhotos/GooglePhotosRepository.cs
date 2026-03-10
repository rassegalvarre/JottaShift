using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using JottaShift.Core.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosRepository(
    GooglePhotosLibraryApiCredentials _apiCredentials,
    IGooglePhotosLibraryFacade photosLibraryFacade,
    IGooglePhotosHttpClient _googlePhotosClient,
    ILogger<GooglePhotosRepository> _logger) : IGooglePhotosRepository
{
    private readonly string[] _scopes = [
        PhotosLibraryService.Scope.PhotoslibraryAppendonly,
        PhotosLibraryService.Scope.PhotoslibraryReadonlyAppcreateddata
    ];

    private PhotosLibraryService? _service;
    private UserCredential? _userCredential;

    public async Task<int> UploadImagesToAlbum(IEnumerable<string> imagesFullPath, string albumName)
    {
        using var photosLibraryService = await GetPhotosLibraryService();
        var album = await GetOrCreateAlbum(albumName);

        var tokens = new List<string>();
        foreach (var image in imagesFullPath)
        {
            string fileName = Path.GetFileName(image);
            var fileData = await File.ReadAllBytesAsync(image);

            var tokenResult = await _googlePhotosClient.UploadPhoto(_userCredential, fileName, fileData);

            if (tokenResult.Succeeded && tokenResult.Value is not null)
            {
                tokens.Add(tokenResult.Value);
            }
            else
            {
                _logger.LogError("Failed to upload image {ImagePath} to Google Photos. Error: {ErrorMessage}",
                    image, tokenResult.ErrorMessage);
            }
        }

        var uploaded = await UploadImages(tokens, album.Id);
        return uploaded?.NewMediaItemResults?.Count ?? 0;
    }

    private async Task<GoogleClientSecrets> GetGoogleClientSecretsAsync()
    {
        if (EnvironmentVariableManager.GooglePhotosLibraryApiProjectId == null ||
            EnvironmentVariableManager.GooglePhotosLibraryApiClientId == null ||
            EnvironmentVariableManager.GooglePhotosLibraryApiClientSecret == null)
        {
            throw new InvalidOperationException("Required environment variables for Google Photos API are not set.");
        }

        _apiCredentials.installed.project_id = EnvironmentVariableManager.GooglePhotosLibraryApiProjectId;
        _apiCredentials.installed.client_id = EnvironmentVariableManager.GooglePhotosLibraryApiClientId;
        _apiCredentials.installed.client_secret= EnvironmentVariableManager.GooglePhotosLibraryApiClientSecret;

        var json = JsonSerializer.Serialize(_apiCredentials);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        
        var secretsResult = await GoogleClientSecrets.FromStreamAsync(stream);
        return secretsResult;
    }

    // TODO: This does not work optimally. Permissions needs to be re-given to often.
    private async Task<UserCredential> GetUserCredential()
    {
        if (_userCredential != null)
        {
            var storedScopes = string.Join(' ', _scopes);
            if (storedScopes != _userCredential.Token.Scope)
            {
                await _userCredential.RevokeTokenAsync(CancellationToken.None);
            }
            else if (_userCredential.Token.IsStale)
            {
                var refreshed = await _userCredential.RefreshTokenAsync(CancellationToken.None);
                if (refreshed)
                {
                    return _userCredential;
                }
            }
            else
            {
                return _userCredential;
            }
        }

        var secretsResult = await GetGoogleClientSecretsAsync();

        // Token will be stored in the token.json folder
        var credPath = "token.json";
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secretsResult.Secrets,
            _scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true));

        _userCredential = credential;
        return _userCredential;
    }

    private async Task<PhotosLibraryService> GetPhotosLibraryService()
    {
        try
        {
            var userCredential = await GetUserCredential();

            _service = new PhotosLibraryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = "JottaShift"
            });


            return _service;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<Album> CreateAlbum(string albumName)
    {
        using var photosLibraryService = await GetPhotosLibraryService();

        var request = new CreateAlbumRequest
        {
            Album = new Album()
            {
                Title = albumName
            }
        };
        var newAlbum = await photosLibraryService.Albums.Create(request).ExecuteAsync();
        _logger.LogInformation("Created new Google Photos album {AlbumName}", albumName);
        
        return newAlbum;
    }

    public async Task<Album> GetOrCreateAlbum(string albumName)
    {
        using var photosLibraryService = await GetPhotosLibraryService();
        var response = await photosLibraryService.Albums.List().ExecuteAsync();
        
        var album = response.Albums?.FirstOrDefault(a => a.Title == albumName);

        album ??= await CreateAlbum(albumName);

        return album;
    }
    
    private async Task<BatchCreateMediaItemsResponse> UploadImages(IEnumerable<string> uploadTokens, string albumId)
    {
        using var photosLibraryService = await GetPhotosLibraryService();

        var albums = await photosLibraryService.Albums.List().ExecuteAsync();

        var created = await photosLibraryService.MediaItems.BatchCreate(new BatchCreateMediaItemsRequest
        {
            NewMediaItems = [.. uploadTokens.Select(t => new NewMediaItem
            {
                SimpleMediaItem = new SimpleMediaItem
                {
                    UploadToken = t                     
                }
            })],
            AlbumId = albumId
        }).ExecuteAsync();

        _logger.LogInformation("Uploaded {ItemCount} items to Google Photos album with id {AlbumId}",
            created.NewMediaItemResults, albumId);

        return created;
    }   
}
