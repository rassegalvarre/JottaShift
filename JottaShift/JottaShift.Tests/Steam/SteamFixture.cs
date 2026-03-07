using JottaShift.Core.Steam;

namespace JottaShift.Tests.Steam;

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
