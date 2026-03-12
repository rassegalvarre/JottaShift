using Google.Apis.Auth.OAuth2;

namespace JottaShift.Core.GooglePhotos;

public interface IUserCredentialManager
{
    Task<Result<UserCredential>> GetUserCredentialAsync();
    Task<Result<string>> GetAccessTokenAsync();
}
