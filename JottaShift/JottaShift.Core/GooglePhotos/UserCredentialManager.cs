using Google.Apis.Auth.OAuth2;
using Google.Apis.PhotosLibrary.v1;
using JottaShift.Core.Configuration;
using System.Text.Json;

namespace JottaShift.Core.GooglePhotos;

public class UserCredentialManager(GooglePhotosLibraryApiCredentials _apiCredentials)
    : IUserCredentialManager
{
    public readonly string CredentialDirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Google.Apis.Auth");

    private readonly string[] _scopes = [
        PhotosLibraryService.Scope.PhotoslibraryAppendonly,
        PhotosLibraryService.Scope.PhotoslibraryReadonlyAppcreateddata
    ];

    private UserCredential? _userCredential;

    public async Task<Result<UserCredential>> GetUserCredentialAsync()
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

        if (EnvironmentManager.GooglePhotosLibraryApiProjectId == null ||
            EnvironmentManager.GooglePhotosLibraryApiClientId == null ||
            EnvironmentManager.GooglePhotosLibraryApiClientSecret == null)
        {
            return Result<UserCredential>.Failure("Required environment variables for Google Photos API are not set.");
        }

        _apiCredentials.installed.project_id = EnvironmentManager.GooglePhotosLibraryApiProjectId;
        _apiCredentials.installed.client_id = EnvironmentManager.GooglePhotosLibraryApiClientId;
        _apiCredentials.installed.client_secret = EnvironmentManager.GooglePhotosLibraryApiClientSecret;

        var json = JsonSerializer.Serialize(_apiCredentials);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        var secretsResult = await GoogleClientSecrets.FromStreamAsync(stream);

        UserCredential newCredentials;
        try
        {
            // Prompt the user to authorize the app
            newCredentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secretsResult.Secrets,
                _scopes,
                "user",
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            return Result<UserCredential>.Failure($"Failed to authorize user credentials: {ex.Message}");
        }

        _userCredential = newCredentials;
        return Result<UserCredential>.Success(_userCredential);
    }

    public async Task<Result<string>> GetAccessTokenAsync()
    {
        var credentialResult = await GetUserCredentialAsync();
        if (!credentialResult.Succeeded || credentialResult.Value is null)
        {
            return Result<string>.Failure(credentialResult.ErrorMessage ?? "Failed to obtain user credentials");
        }

        return Result<string>.Success(credentialResult.Value.Token.AccessToken);
    }
}
