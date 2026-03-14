using JottaShift.Core.Configuration;

namespace JottaShift.Tests.Configuration;

public class EnvironmentManagerTests
{
    [Fact]
    [Trait("Dependency", "Env")]
    public void GetKnownMachineName_ShouldReturnKnownName()
    {
        var result = EnvironmentManager.GetKnownMachineName();

        ResultAssert.ValueSuccess(result, "BRUTUS");
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
