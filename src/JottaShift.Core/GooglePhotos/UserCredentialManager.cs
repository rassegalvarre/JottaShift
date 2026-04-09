using Google.Apis.Auth.OAuth2;
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
        "https://www.googleapis.com/auth/photoslibrary.appendonly",
        "https://www.googleapis.com/auth/photoslibrary.readonly.appcreateddata"
    ];

    private UserCredential? _userCredential;

    private Result PopulateCredentials(GooglePhotosLibraryApiCredentials credentials)
    {
        if (EnvironmentManager.TryGetEnvironmentVariable(
            EnvironmentManager.GooglePhotosLibraryApiProjectId, out string projectId) &&
            EnvironmentManager.TryGetEnvironmentVariable(
            EnvironmentManager.GooglePhotosLibraryApiClientId, out string clientId) &&
            EnvironmentManager.TryGetEnvironmentVariable(
            EnvironmentManager.GooglePhotosLibraryApiClientSecret, out string clientSecret))
        {
            _apiCredentials.installed.project_id = projectId;
            _apiCredentials.installed.client_id = clientId;
            _apiCredentials.installed.client_secret = clientSecret;

            return Result.Success();
        }

        return Result.Failure("Missing environment variable for Google API");
    }

    private static async Task<Result> RefreshTokenAsync(UserCredential userCredential)
    {
        if (userCredential.Token.IsStale)
        {
            var refreshed = await userCredential.RefreshTokenAsync(CancellationToken.None);
            if (!refreshed)
            {
                return Result.Failure("Could not refresh user credentials");
            }
        }

        return Result.Success();
    }

    public async Task<Result<UserCredential>> GetUserCredentialAsync()
    {
        // Return instance-credentials if available
        if (_userCredential != null)
        {            
            return Result<UserCredential>.Success(_userCredential);
        }

        // Create new credentials
        var credentialsResult = PopulateCredentials(_apiCredentials);
        if (!credentialsResult.Succeeded)
        {
            return credentialsResult.ForwardFailure<UserCredential>();
        }

        var json = JsonSerializer.Serialize(_apiCredentials);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        var secretsResult = await GoogleClientSecrets.FromStreamAsync(stream);

        UserCredential newCredentials;
        try
        {
            // AuthorizeAsync will either localte existing credentials in AppData/Roaming if it exists.
            // If not, the user will be prompted to authorize the app. Credentials will then be stored in AppData/Roaming for future use.
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

        var refreshResult = await RefreshTokenAsync(credentialResult.Value);
        if (!refreshResult.Succeeded)
        {
            return refreshResult.ForwardFailure<string>();
        }

        return Result<string>.Success(credentialResult.Value.Token.AccessToken);
    }
}
