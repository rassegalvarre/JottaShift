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

    public static string GetAppSettingsFileFullPath(bool? useMachineDefinedSettings = false)
    {
        string baseAppSettingsFileName = "appsettings.json";
        string appSettingsFullPath = Path.Combine(AppSettingsBaseFullPath, baseAppSettingsFileName);

        var machineNameResult = EnvironmentManager.GetKnownMachineName();
        if (useMachineDefinedSettings == true &&
            machineNameResult.Succeeded &&
            machineNameResult.Value is not null)
        {
            string machineDefinedAppSettingPath = Path.Combine(AppSettingsBaseFullPath, $"appsettings.{machineNameResult.Value}.json");
            
            if (File.Exists(machineDefinedAppSettingPath))
            {
                appSettingsFullPath = machineDefinedAppSettingPath;
            }
        }

        return appSettingsFullPath;
    }

    public required FileExportJobs FileExportJobs { get; init; }
    public required GooglePhotosLibraryApiCredentials GooglePhotosLibraryApiCredentials { get; init; }
    public required JottacloudSettings JottacloudSettings { get; init; }
    public required SteamWebApiCredentials SteamWebApiCredentials { get; init; }
}
