using JottaShift.Core.SteamRepository;

namespace JottaShift.Tests;

public class SteamFixture : IDisposable
{
    public SteamWebApiCredentials SteamWebApiCredentialsMock => new()
    {
        api_key = string.Empty,
        domain_name = string.Empty,
        store_language = string.Empty
    };

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
