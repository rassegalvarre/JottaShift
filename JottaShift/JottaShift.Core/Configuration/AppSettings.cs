using JottaShift.Core.GooglePhotos;
using JottaShift.Core.Steam;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.FileExport.Jobs;

namespace JottaShift.Core.Configuration;

public class AppSettings
{
    public static string AppSettingsBaseFullPath => Path.Combine(
        AppContext.BaseDirectory,
        "Configuration");

    public static string GetAppSettingsFileFullPath()
    {
        string appSettingsFileName = "appsettings.json";
        var machineNameResult = EnvironmentManager.GetKnownMachineName();
        if (machineNameResult.Succeeded && machineNameResult.Value is not null)
        {
            appSettingsFileName = $"appsettings.{machineNameResult.Value}.json";
        }

        return Path.Combine(AppSettingsBaseFullPath, appSettingsFileName);
    }

    public required FileExportJobs FileExportJobs { get; init; }
    public required GooglePhotosLibraryApiCredentials GooglePhotosLibraryApiCredentials { get; init; }
    public required JottacloudSettings JottacloudSettings { get; init; }
    public required SteamWebApiCredentials SteamWebApiCredentials { get; init; }
}
