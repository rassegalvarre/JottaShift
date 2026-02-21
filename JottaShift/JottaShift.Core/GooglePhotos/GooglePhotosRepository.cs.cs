using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosRepository : IGooglePhotos
{
    private PhotosLibraryService? _service;

    // TODO: Remove once ready
    private static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string Duck = Path.Combine(TestDataPath, "duck.jpg");

    public UserCredential Credential()
    {
        string credentialsPath = Path.Combine(AppContext.BaseDirectory, "GooglePhotos", "credentials.json");
        if (!File.Exists(credentialsPath))
            throw new FileNotFoundException("Credentials.json not found");

        string[] scopes = {
            PhotosLibraryService.Scope.PhotoslibraryAppendonly,
            PhotosLibraryService.Scope.PhotoslibraryReadonlyAppcreateddata
        };
        using var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read);
        var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

        // Token will be stored in the token.json folder
        var credPath = "token.json";
        var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true)).Result;

        return credential;
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

    public async Task<bool> CreateAlbum()
    {
        var credential = Credential();
        using var photosLibraryService = GetPhotosLibraryService(credential);

        var newAlbum = new CreateAlbumRequest
        {
            Album = new Album()
            {
                Title = "Lorem ipsum" // TODO: Add album-name to appsettings.
            }
        };
        Album created = await photosLibraryService.Albums.Create(newAlbum).ExecuteAsync();

        return created != null;
    }

    // TODO: Add method "UploadImagesFromStaging"
    // TODO: Add method ClearStagedImages
    public async Task<bool> UploadImage()
    {
        var credential = Credential();
        using var photosLibraryService = GetPhotosLibraryService(credential);

        var token = await UploadBytesAsync(credential, Duck);

        var albums = await photosLibraryService.Albums.List().ExecuteAsync();

        var created = await photosLibraryService.MediaItems.BatchCreate(new BatchCreateMediaItemsRequest
        {
            NewMediaItems = new List<NewMediaItem>
            {
                new NewMediaItem
                {
                    Description = "A duck",
                    SimpleMediaItem = new SimpleMediaItem
                    {
                        UploadToken = token                     
                    },

                }
            },
            AlbumId = albums.Albums.FirstOrDefault()?.Id // TODO: Filter on album defined in appsettings
        }).ExecuteAsync();

        return created != null;
    }

    private static async Task<string> UploadBytesAsync(UserCredential cred, string filePath)
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
