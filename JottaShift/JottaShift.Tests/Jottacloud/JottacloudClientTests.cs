using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Jottacloud;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudClientTests(
    JottacloudFixture _fixture,
    HttpClientWrapperFixture _httpClientFixture) :
    IClassFixture<JottacloudFixture>,
    IClassFixture<HttpClientWrapperFixture>
{
    [Fact]
    [Trait("API", "Jottacloud")]
    public async Task GetAlbumAsync_ReturnsAlbum_WhenValidAlbumId()
    {
        var httpClientWrapper = _httpClientFixture.CreateHttpClientWrapper();
        var client = _fixture.CreateJottacloudClient(httpClientWrapper);
        
        var albumResponse = await client.GetAlbumAsync(JottacloudFixture.Settings.TestAlbumId);

        Assert.NotNull(albumResponse);
        Assert.NotEmpty(albumResponse.Photos);
    }

    [Fact]
    public async Task GetAlbumAsync_ReturnsUnsuccessfullResult_WhenRequestThrowsException()
    {
        var mockHttpWrapper = new Mock<IHttpClientWrapper>();

        mockHttpWrapper
            .Setup(h => h.GetAsync<Album>(It.IsAny<string>()))
            .ReturnsAsync(new HttpGetResult<Album>(System.Net.HttpStatusCode.BadRequest));

        var client = new JottacloudClient(
            mockHttpWrapper.Object,
            new Mock<ILogger<JottacloudClient>>().Object);


        await Assert.ThrowsAsync<Exception>(async () => await client.GetAlbumAsync(JottacloudFixture.Settings.TestAlbumId));
    }
}
