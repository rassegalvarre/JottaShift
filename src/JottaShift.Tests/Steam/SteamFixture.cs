using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Steam;
using JottaShift.Tests.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.Steam;

public class SteamFixture : AppSettingsFixture
{
    public async Task<SteamRepository> CreateSteamRepository(IHttpClientWrapper? httpClientWrapper = null)
    {
        httpClientWrapper ??= new Mock<IHttpClientWrapper>().Object;
        var appSettings = await GetAppSettingsAsync();

        return new SteamRepository(
            new Mock<ILogger<SteamRepository>>().Object,
            httpClientWrapper,
            appSettings.SteamWebApiCredentials);
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
