using Microsoft.Extensions.Logging;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace JottaShift.Core.SteamRepository;

public class SteamRepository(ILogger<SteamRepository> _logger) : ISteamRepository
{
    public async Task<string> GetAppNameFromId(uint appId)
    {
        var web = new SteamStore(new HttpClient());

        string appName = string.Empty;

        try
        {
            var result = await web.GetStoreAppDetailsAsync(appId, language: "no"); // TODO: Get from config
            appName = result?.Name ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occured when fetching name for app with id {Appid}. ExceptionMessage: {Message}", appId, ex.Message);
        }

        return appName;
    }
}