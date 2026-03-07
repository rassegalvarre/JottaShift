using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests;

[Trait("API", "Google")]
public class SteamRepositoryTests(SteamFixture _fixture) : IClassFixture<SteamFixture>
{
    [Fact]
    public async Task GetAppNameFromId_ReturnsName_WhenValidId()
    {
        var steamRepository = new SteamRepository(
            new Mock<ILogger<SteamRepository>>().Object,
            _fixture.SteamWebApiCredentialsMock);

        const uint appId = 990080;
        const string appName = "Hogwarts Legacy";

        var result = await steamRepository.GetAppNameFromId(appId);

        Assert.Equal(appName, result);
    }

    [Fact]
    public async Task GetGameName_ReturnsEmptyString_WhenInvalidId()
    {
        var steamRepository = new SteamRepository(
            new Mock<ILogger<SteamRepository>>().Object,
            _fixture.SteamWebApiCredentialsMock);

        const uint appId = 0;

        var result = await steamRepository.GetAppNameFromId(appId);

        Assert.Equal(string.Empty, result);
    }
}
