namespace JottaShift.Core.SteamRepository;

public interface ISteamRepository
{
    Task<string> GetAppNameFromId(uint appId);
}
