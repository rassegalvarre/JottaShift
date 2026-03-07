using JottaShift.Core.Configuration;
using Microsoft.Extensions.Logging;
using SteamWebAPI2.Interfaces;

namespace JottaShift.Core.SteamRepository;

public class SteamRepository(
    ILogger<SteamRepository> logger,
    SteamWebApiCredentials webApiCredentials) : ISteamRepository
{
    private readonly ILogger<SteamRepository> _logger = logger;
    private const string FallbackStoreLanguage = "en";

    public async Task<string> GetAppNameFromId(uint appId)
    {
        var web = new SteamStore(new HttpClient());

        string appName = string.Empty;

        try
        {
            var result = await web.GetStoreAppDetailsAsync(appId, language: webApiCredentials.store_language);
            appName = result?.Name ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception occured when fetching name for app with id {Appid}. ExceptionMessage: {Message}", appId, ex.Message);
        }

        return appName;
    }
}