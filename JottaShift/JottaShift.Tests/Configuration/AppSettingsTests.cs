using JottaShift.Core.Configuration;

namespace JottaShift.Tests.Configuration;

public class AppSettingsTests(AppSettingsFixture _fixture) : IClassFixture<AppSettingsFixture>
{
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
