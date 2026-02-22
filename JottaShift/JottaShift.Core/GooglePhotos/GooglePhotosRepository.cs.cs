using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosRepository : IGooglePhotos
{
    public const string AlbumName = "Chromecast";    

    private readonly string[] _scopes = [
        PhotosLibraryService.Scope.PhotoslibraryAppendonly,
        PhotosLibraryService.Scope.PhotoslibraryReadonlyAppcreateddata
    ];

    private PhotosLibraryService? _service;
    private UserCredential? _userCredential;
    
    private async Task<UserCredential> CreateUserCredential()
    {      
        string credentialsPath = Path.Combine(AppContext.BaseDirectory, "GooglePhotos", "credentials.json");
        if (!File.Exists(credentialsPath))
            throw new FileNotFoundException("Credentials.json not found");
        
        using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
        var secretsResult = await GoogleClientSecrets.FromStreamAsync(stream);


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

    private async Task<UserCredential> UserCredential()
    {
        if (_userCredential == null)
        {
            return await CreateUserCredential();
        }

        if (_userCredential.Token.IsStale)
        {
            var refreshed = await _userCredential.RefreshTokenAsync(CancellationToken.None);
            if (!refreshed)
            {
                return await CreateUserCredential();
            }
        }

        if (_scopes.Contains(_userCredential.Token.Scope))
        {
            await _userCredential.RevokeTokenAsync(CancellationToken.None);
            return await CreateUserCredential();
        }

        return _userCredential;
        
    }

    public PhotosLibraryService GetPhotosLibraryService(UserCredential userCredential)
    {
        try
        {
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
        var credential = await UserCredential();
        using var photosLibraryService = GetPhotosLibraryService(credential);

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
        var credential = await UserCredential();
        using var photosLibraryService = GetPhotosLibraryService(credential);
        var response = await photosLibraryService.Albums.List().ExecuteAsync();
        
        var album = response.Albums?.FirstOrDefault(a => a.Title == albumName);

        album ??= await CreateAlbum(albumName);

        return album;
    }

    public async Task<bool> UploadImageToAlbum(IEnumerable<string> imagesFullPath, string albumName)
    {
        var credential = await UserCredential();
        using var photosLibraryService = GetPhotosLibraryService(credential);
        var album = await GetOrCreateAlbum(albumName);

        var tokens = new List<string>();
        foreach (var image in imagesFullPath)
        {
            var token = await UploadFileToGoogleStorage(credential, image);
            tokens.Add(token);
        }

        var uploaded = await UploadImages(tokens, album.Id);
        return uploaded != null;
    }

    // TODO: Add method "UploadImagesFromStaging"
    // TODO: Add method ClearStagedImages
    // Note: Should those be in this repo, or TimelineService (rename to StagingService?)
    private async Task<BatchCreateMediaItemsResponse> UploadImages(IEnumerable<string> uploadTokens, string albumId)
    {
        var credential = await UserCredential();
        using var photosLibraryService = GetPhotosLibraryService(credential);

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

    private static async Task<string> UploadFileToGoogleStorage(UserCredential cred, string filePath)
    {
        const string uploadUrl = "https://photoslibrary.googleapis.com/v1/uploads";

        byte[] data = await File.ReadAllBytesAsync(filePath);
        var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, uploadUrl)
        {
            Content = new System.Net.Http.ByteArrayContent(data)
        };

        // Required headers (see Google docs)
        request.Headers.Add("Authorization", $"Bearer {cred.Token.AccessToken}");
        request.Headers.Add("X-Goog-Upload-File-Name", Path.GetFileName(filePath));
        request.Headers.Add("X-Goog-Upload-Protocol", "raw");
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        var http = new System.Net.Http.HttpClient();
        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // The body of a successful response is just the upload token (plain text)
        string uploadToken = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Upload token received: {uploadToken.Substring(0, Math.Min(10, uploadToken.Length))}…");
        return uploadToken;
    }
}
