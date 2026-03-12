using JottaShift.Core.Configuration;

namespace JottaShift.Tests.Configuration;

public class EnvironmentVariableManagerTests
{
    [Fact]
    [Trait("Dependency", "Env")]
    public void ValidateAllEnvironmentVariablesForApplication()
    {
        Assert.False(string.IsNullOrEmpty(
            EnvironmentVariableManager.GooglePhotosLibraryApiClientId));

        Assert.False(string.IsNullOrEmpty(
            EnvironmentVariableManager.GooglePhotosLibraryApiClientSecret));

        Assert.False(string.IsNullOrEmpty(
            EnvironmentVariableManager.GooglePhotosLibraryApiProjectId));

        Assert.False(string.IsNullOrEmpty(
            EnvironmentVariableManager.SteamWebApiClientApiKey));

        Assert.False(string.IsNullOrEmpty(
            EnvironmentVariableManager.SteamWebApiStoreLanguage));
    }
}
