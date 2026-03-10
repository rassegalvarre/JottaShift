using JottaShift.Core.GooglePhotos;
using JottaShift.Core.Steam;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.FileExport.Jobs;

namespace JottaShift.Core.Configuration;

public class AppSettings
{
    public required FileExportJobs FileExportSettings { get; init; }
    public required GooglePhotosLibraryApiCredentials GooglePhotosLibraryApiCredentials { get; init; }
    public required JottacloudSettings JottacloudSettings { get; init; }
    public required SteamWebApiCredentials SteamWebApiCredentials { get; init; }
}
