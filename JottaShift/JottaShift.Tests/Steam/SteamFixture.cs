using JottaShift.Core.Steam;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.Steam;

public class SteamFixture : IDisposable
{
    public SteamWebApiCredentials SteamWebApiCredentialsMock => new()
    {
        api_key = string.Empty,
        domain_name = string.Empty,
        store_language = string.Empty
    };

    public SteamRepository CreateSteamRepository()
    {
        return new SteamRepository(
            new Mock<ILogger<SteamRepository>>().Object,
            SteamWebApiCredentialsMock);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
