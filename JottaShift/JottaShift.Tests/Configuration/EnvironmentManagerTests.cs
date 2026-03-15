using JottaShift.Core.Configuration;

namespace JottaShift.Tests.Configuration;

public class EnvironmentManagerTests
{
    [Fact]
    [Trait("Dependency", "Env")]
    public void GetKnownMachineName_ShouldReturnKnownNameForCurrentMachine()
    {
        var result = EnvironmentManager.GetKnownMachineName();

        ResultAssert.ValueSuccess(result, Environment.MachineName);
    }

    [Fact]
    [Trait("Dependency", "Env")]
    public void ValidateAllEnvironmentVariablesForApplication()
    {
        Assert.False(string.IsNullOrEmpty(
            EnvironmentManager.GooglePhotosLibraryApiClientId));

        Assert.False(string.IsNullOrEmpty(
            EnvironmentManager.GooglePhotosLibraryApiClientSecret));

        Assert.False(string.IsNullOrEmpty(
            EnvironmentManager.GooglePhotosLibraryApiProjectId));

        Assert.False(string.IsNullOrEmpty(
            EnvironmentManager.SteamWebApiClientApiKey));

        Assert.False(string.IsNullOrEmpty(
            EnvironmentManager.SteamWebApiStoreLanguage));
    }
}
