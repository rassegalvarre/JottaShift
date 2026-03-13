namespace JottaShift.Core.Steam;

public interface ISteamRepository
{
    Task<Result<string>> GetAppNameFromId(uint appId);
}
