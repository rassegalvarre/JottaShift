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

    [Theory]
    [InlineData(EnvironmentManager.GooglePhotosLibraryApiProjectId)]
    [InlineData(EnvironmentManager.GooglePhotosLibraryApiClientId)]
    [InlineData(EnvironmentManager.GooglePhotosLibraryApiClientSecret)]
    [InlineData(EnvironmentManager.SteamWebApiClientApiKey)]
    [InlineData(EnvironmentManager.SteamWebApiStoreLanguage)]
    [Trait("Dependency", "Env")]
    public void ValidateAllEnvironmentVariablesForApplication(string key)
    {
        var found = EnvironmentManager.TryGetEnvironmentVariable(key, out string value);

        Assert.True(found, $"Environment variable {key} is not set");
        Assert.False(string.IsNullOrEmpty(value));
    }
}
