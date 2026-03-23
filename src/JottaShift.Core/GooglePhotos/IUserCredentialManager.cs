using Google.Apis.Auth.OAuth2;

namespace JottaShift.Core.GooglePhotos;

public interface IUserCredentialManager
{
    /// <summary>
    /// Will authorize the app to a user.
    /// The user will be prompted for authorization the first time, and a token will be saved locally.
    /// The local token will then be refreshed automatically when authorization is invoked again. 
    /// </summary>
    Task<Result<UserCredential>> GetUserCredentialAsync();
    Task<Result<string>> GetAccessTokenAsync();
}
