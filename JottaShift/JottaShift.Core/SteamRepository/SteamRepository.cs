using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace JottaShift.Core.SteamRepository;

public class SteamRepository : ISteamRepository
{
    public async Task<string> GetAppNameFromId(uint id)
    {
        const string apiKey = ""; // DONT COMMIT
        var factory = new SteamWebInterfaceFactory(apiKey);
        var web = new SteamStore(new HttpClient());

        var result = await web.GetStoreAppDetailsAsync(id, language: "no"); // TODO: Make language configurable

        return result.Name;
    }
}