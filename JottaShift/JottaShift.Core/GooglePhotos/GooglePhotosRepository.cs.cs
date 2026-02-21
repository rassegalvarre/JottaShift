using System;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosRepository : IGooglePhotos
{
    private PhotosLibraryService? _service;

    public PhotosLibraryService GetPhotosLibraryService()
    {
        try
        {
            // Path to client credentials JSON obtained from Google Cloud Console
            const string credentialsPath = @"C:\Users\krist\Downloads\credentials.json";
            if (!File.Exists(credentialsPath))
                throw new FileNotFoundException("Credentials.json not found");

            string[] scopes = { PhotosLibraryService.Scope.PhotoslibraryReadonly };
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

            if (credential == null)
            {
                throw new UnauthorizedAccessException("Could not create credentials");
            }

            _service = new PhotosLibraryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "JottaShift"
            });


            return _service;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
