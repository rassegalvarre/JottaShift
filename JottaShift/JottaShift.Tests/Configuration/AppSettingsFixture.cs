using JottaShift.Core.Configuration;
using System.Text.Json;

namespace JottaShift.Tests.Configuration;

public class AppSettingsFixture : IDisposable
{
    public async Task<AppSettings> GetAppSettingsAsync()
    {
        var appSettingsFileStream = File.OpenRead(AppSettings.AppSettingsFullPath);
        var appSettingsInstance = await JsonSerializer.DeserializeAsync<AppSettings>(appSettingsFileStream);

        return appSettingsInstance ??
            throw new FileLoadException("Could not deserialize the expected appsettings.json file");
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
