namespace JottaShift.Core.Steam;

public interface ISteamRepository
{
    Task<string> GetAppNameFromId(uint appId);
}
