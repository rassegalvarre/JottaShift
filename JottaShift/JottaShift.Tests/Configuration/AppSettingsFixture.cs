using JottaShift.Core.Configuration;
using System.Text.Json;

namespace JottaShift.Tests.Configuration;

public class AppSettingsFixture : IDisposable
{
    private AppSettings? _appSettings;

    public async Task<AppSettings> GetAppSettingsAsync()
    {
        if (_appSettings is not null)
        {
            return _appSettings;
        }

        var appSettingsFileStream = File.OpenRead(AppSettings.AppSettingsFullPath);
        var appSettingsInstance = await JsonSerializer.DeserializeAsync<AppSettings>(appSettingsFileStream);

        _appSettings = appSettingsInstance;

        return _appSettings ??
            throw new FileLoadException("Could not deserialize the expected appsettings.json file");
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
