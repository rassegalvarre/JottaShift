using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using JottaShift.Core.Configuration;
using System.IO.Abstractions;
using System.Text.Json;

namespace JottaShift.Core.GooglePhotos;

// TODO: Add logger and logs
public class GooglePhotosRepository(IFileSystem _fileSystem) : IGooglePhotosRepository
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
            var token = await UploadFileToGoogleStorage(image);
            tokens.Add(token);
        }

        var uploaded = await UploadImages(tokens, album.Id);
        return uploaded?.NewMediaItemResults?.Count ?? 0;
    }

    private async Task<GoogleClientSecrets> GetGoogleClientSecretsAsync()
    {
        string credentialsPath = Path.Combine(AppContext.BaseDirectory, "google-api-credentials.json");
        if (!_fileSystem.File.Exists(credentialsPath))
            throw new FileNotFoundException("google-api-credentials not found");

        string fileContent = await _fileSystem.File.ReadAllTextAsync(credentialsPath);

        var apiCredentials = JsonSerializer.Deserialize<GooglePhotosLibraryApi>(fileContent) ??
            throw new InvalidOperationException("Failed to deserialize Google API credentials.");

        if (EnvironmentVariableManager.GooglePhotosLibraryApiProjectId == null ||
            EnvironmentVariableManager.GooglePhotosLibraryApiClientId == null ||
            EnvironmentVariableManager.GooglePhotosLibraryApiClientSecret == null)
        {
            throw new InvalidOperationException("Required environment variables for Google Photos API are not set.");
        }

        apiCredentials.Installed.ProjectId = EnvironmentVariableManager.GooglePhotosLibraryApiProjectId;
        apiCredentials.Installed.ClientId = EnvironmentVariableManager.GooglePhotosLibraryApiClientId;
        apiCredentials.Installed.ClientSecret = EnvironmentVariableManager.GooglePhotosLibraryApiClientSecret;

        var json = JsonSerializer.Serialize(apiCredentials);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        
        var secretsResult = await GoogleClientSecrets.FromStreamAsync(stream);
        return secretsResult;
    }

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

        return created;
    }

    // AI-generated. Should be tested and probably refactored.
    private async Task<string> UploadFileToGoogleStorage(string filePath)
    {
        const string uploadUrl = "https://photoslibrary.googleapis.com/v1/uploads";

        byte[] data = await File.ReadAllBytesAsync(filePath);
        var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, uploadUrl)
        {
            Content = new System.Net.Http.ByteArrayContent(data)
        };

        if (_userCredential == null)
        {
            throw new InvalidOperationException("User credential is not available. Please authenticate first.");
        }

        // Required headers (see Google docs)
        request.Headers.Add("Authorization", $"Bearer {_userCredential.Token.AccessToken}");
        request.Headers.Add("X-Goog-Upload-File-Name", Path.GetFileName(filePath));
        request.Headers.Add("X-Goog-Upload-Protocol", "raw");
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        var http = new System.Net.Http.HttpClient();
        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // The body of a successful response is just the upload token (plain text)
        string uploadToken = await response.Content.ReadAsStringAsync();
        return uploadToken;
    }
}
