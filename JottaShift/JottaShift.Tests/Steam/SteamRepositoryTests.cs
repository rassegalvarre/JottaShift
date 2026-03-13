namespace JottaShift.Tests.Steam;

[Trait("API", "Google")]
public class SteamRepositoryTests(SteamFixture _fixture, HttpClientWrapperFixture _httpFixture) : 
    IClassFixture<SteamFixture>,
    IClassFixture<HttpClientWrapperFixture>
{
    [Fact]
    [Trait("Dependency", "Steam.Api")]
    public async Task GetAppNameFromId_ReturnsName_WhenValidId()
    {
        const uint appId = 990080;
        const string appName = "Hogwarts Legacy";

        var steamRepository = _fixture.CreateSteamRepository(
            _httpFixture.CreateHttpClientWrapper());

        var result = await steamRepository.GetAppNameFromId(appId);

        ResultAssert.ValueSuccess(result, appName);
    }

    [Fact]
    [Trait("Dependency", "Steam.Api")]
    public async Task GetGameName_ReturnsEmptyString_WhenInvalidId()
    {
        var steamRepository = _fixture.CreateSteamRepository(
            _httpFixture.CreateHttpClientWrapper());

        var result = await steamRepository.GetAppNameFromId(0);

        ResultAssert.ValueFailure(result);
    }
}
