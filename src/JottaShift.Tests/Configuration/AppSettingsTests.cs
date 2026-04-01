using JottaShift.Core.Configuration;

namespace JottaShift.Tests.Configuration;

public class AppSettingsTests(AppSettingsFixture _fixture) : IClassFixture<AppSettingsFixture>
{
    [Fact]
    [Trait("Dependency", "Env")]
    public async Task GetAppSettingsFileFullPath_ShouldReturnSettingsForCurrentMachine()
    {
        var fullPath = AppSettings.GetAppSettingsFileFullPath(true);

        Assert.True(Path.IsPathFullyQualified(fullPath));
        Assert.Contains(Environment.MachineName, fullPath);
    }
    
    [Fact]
    public async Task GetAppSettingsFileFullPath_ShouldReturnDefaultSettings()
    {
        var fullPath = AppSettings.GetAppSettingsFileFullPath(false);

        Assert.True(Path.IsPathFullyQualified(fullPath));
        Assert.Contains("appsettings.json", fullPath);
    }


    [Fact]
    public async Task GetAppSettingsAsync_ShouldReturn_DeserializedInstance()
    {
        var appSettingsInstance = await _fixture.GetAppSettingsAsync();

        Assert.NotNull(appSettingsInstance);
        Assert.IsType<AppSettings>(appSettingsInstance);

        Assert.NotNull(appSettingsInstance.FileExportJobs);
        Assert.NotNull(appSettingsInstance.GooglePhotosLibraryApiCredentials.installed.token_uri);
        Assert.NotNull(appSettingsInstance.JottacloudSettings.TestAlbumId);
        Assert.NotNull(appSettingsInstance.SteamWebApiCredentials.store_language);
    }
}
