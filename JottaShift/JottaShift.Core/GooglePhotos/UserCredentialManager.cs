using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using Google.Apis.Util.Store;
using JottaShift.Core.Configuration;
using System.Text.Json;

namespace JottaShift.Core.GooglePhotos;

public class UserCredentialManager(GooglePhotosLibraryApiCredentials _apiCredentials) : IUserCredentialManager
{
    private readonly string[] _scopes = [
        PhotosLibraryService.Scope.PhotoslibraryAppendonly,
        PhotosLibraryService.Scope.PhotoslibraryReadonlyAppcreateddata
    ];

    private UserCredential? _userCredential;

    // TODO: Try to read .json-file in token.json
    public async Task<Result<UserCredential>> GetCredentialAsync()
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

        if (EnvironmentVariableManager.GooglePhotosLibraryApiProjectId == null ||
            EnvironmentVariableManager.GooglePhotosLibraryApiClientId == null ||
            EnvironmentVariableManager.GooglePhotosLibraryApiClientSecret == null)
        {
            return Result<UserCredential>.Failure("Required environment variables for Google Photos API are not set.");
        }

        _apiCredentials.installed.project_id = EnvironmentVariableManager.GooglePhotosLibraryApiProjectId;
        _apiCredentials.installed.client_id = EnvironmentVariableManager.GooglePhotosLibraryApiClientId;
        _apiCredentials.installed.client_secret = EnvironmentVariableManager.GooglePhotosLibraryApiClientSecret;

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

}
