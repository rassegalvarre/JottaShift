using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Tests.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;

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
    public async Task GetUserCredentialAsync_ShouldPromptAuthorization_WhenNotJsonTokenFound()
    {
        var userCredentialManager = await GetUserCredentialManagerAsync();
        
        // Ensure that no token is saved locally
        if (Directory.Exists(userCredentialManager.CredentialDirectoryPath))
        {
            Directory.Delete(userCredentialManager.CredentialDirectoryPath, true);
        }

        // Act
        var userCredentialResult = await userCredentialManager.GetUserCredentialAsync();

        // Assert that the user was prompted, authorized the app and that the token was saved locally
        Assert.True(userCredentialResult.Succeeded);
        Assert.NotNull(userCredentialResult.Value);

        Assert.True(
            Directory.Exists(userCredentialManager.CredentialDirectoryPath));

        Assert.Single(
            Directory.GetFiles(userCredentialManager.CredentialDirectoryPath));
    }
}
