using JottaShift.Core.GooglePhotos;
using JottaShift.Tests.Configuration;

namespace JottaShift.Tests.GooglePhotos;

[Trait("Dependency", "Google.Api")]
public class UserCredentialManagerTests(AppSettingsFixture _appSettingsFixture)
    : IClassFixture<AppSettingsFixture>
{
    private async Task<UserCredentialManager> GetUserCredentialManagerAsync()
    {
        var appSettings = await _appSettingsFixture.GetAppSettingsAsync();
        return new UserCredentialManager(appSettings.GooglePhotosLibraryApiCredentials);
    }

    [Fact]
    public async Task GetUserCredentialAsync_ShouldAuthorizeAndStoreToken()
    {
        var userCredentialManager = await GetUserCredentialManagerAsync();
               
        // Act
        // The user will be prompted for authorization unless a token exists locally.
        var userCredentialResult = await userCredentialManager.GetUserCredentialAsync();

        // Assert that the app was authorized and the token was saved locally
        ResultAssert.Success(userCredentialResult);

        Assert.True(
            Directory.Exists(userCredentialManager.CredentialDirectoryPath));

        Assert.Single(
            Directory.GetFiles(userCredentialManager.CredentialDirectoryPath));


        // Assert that GetAccessTokenAsync return the same token when quried before expiration
        var accessToken = await userCredentialManager.GetAccessTokenAsync();
        ResultAssert.ValueSuccess(accessToken,
            userCredentialResult.Value!.Token.AccessToken);
    }
}
