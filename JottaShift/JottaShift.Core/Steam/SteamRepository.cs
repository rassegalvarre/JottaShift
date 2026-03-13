using Microsoft.Extensions.Logging;
using SteamWebAPI2.Interfaces;

namespace JottaShift.Core.Steam;

public class SteamRepository(
    ILogger<SteamRepository> logger,
    SteamWebApiCredentials webApiCredentials) : ISteamRepository
{
    private readonly ILogger<SteamRepository> _logger = logger;

    private const string FallbackStoreLanguage = "en";

    public async Task<Result<string>> GetAppNameFromId(uint appId)
    {
        try
        {
            var steamStoreFacade = new SteamStore(new HttpClient()); // TODO: Inject HttpClient

            string storeLanguage = !string.IsNullOrEmpty(webApiCredentials.store_language) ?
                webApiCredentials.store_language :
                FallbackStoreLanguage;

            var appDetailsResult = await steamStoreFacade.GetStoreAppDetailsAsync(appId,
                language: webApiCredentials.store_language ?? FallbackStoreLanguage);

            if (appDetailsResult is null || string.IsNullOrEmpty(appDetailsResult.Name))
            {
                _logger.LogError("Did not find app with Id {AppId}", appId);
                return Result<string>.Failure("App not found");
            }

            return Result<string>.Success(appDetailsResult.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception occured when fetching name for app with id {AppId}", appId);
            return Result<string>.Failure("An error occured when getting app details");
        }
    }
}