using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.Jottacloud.Models.Domain;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudHttpClientTests(
    JottacloudFixture _fixture,
    HttpClientWrapperFixture _httpClientFixture) :
    IClassFixture<JottacloudFixture>,
    IClassFixture<HttpClientWrapperFixture>
{
    [Fact]
    [Trait("Dependency", "Jottacloud.Api")]
    public async Task GetAlbumAsync_WithImplementedHttpClient_ReturnsAlbum_WhenValidAlbumIdUsingHttpClient()
    {
        var httpClientWrapper = _httpClientFixture.CreateHttpClientWrapper();
        var client = _fixture.CreateJottacloudClient(httpClientWrapper);
        
        var albumResult = await client.GetAlbumAsync(_fixture.Settings.TestAlbumId);

        ResultAssert.Success(albumResult);
        Assert.NotEmpty(albumResult.Value!.Photos);
    }

    [Fact]
    public async Task GetAlbumAsync_WithMockedHttpClient_ReturnsAlbum_WhenValidAlbumId()
    {
        var expectedPhoto = new Photo() { Filename = Path.GetRandomFileName() };
        var expectedAlbumResponse = new Album()
        {
            Id = Guid.NewGuid().ToString(),
            Photos = [expectedPhoto]
        };

        var httpClientWrapper = new Mock<IHttpClientWrapper>();
        httpClientWrapper.Setup(w => w.GetAsync<Album>(It.IsAny<string>()))
            .ReturnsAsync(new HttpGetResult<Album>(
                System.Net.HttpStatusCode.OK, expectedAlbumResponse));

        var client = _fixture.CreateJottacloudClient(httpClientWrapper.Object);

        var albumResult = await client.GetAlbumAsync(_fixture.Settings.TestAlbumId);

        ResultAssert.ValueSuccess(albumResult, expectedAlbumResponse);
    }

    [Fact]
    public async Task GetAlbumAsync_ReturnsUnsuccessfullResult_WhenRequestThrowsException()
    {
        var mockHttpWrapper = new Mock<IHttpClientWrapper>();

        mockHttpWrapper
            .Setup(h => h.GetAsync<Album>(It.IsAny<string>()))
            .ReturnsAsync(new HttpGetResult<Album>(System.Net.HttpStatusCode.BadRequest));

        var client = new JottacloudHttpClient(
            mockHttpWrapper.Object,
            new Mock<ILogger<JottacloudHttpClient>>().Object);


        var albumResult = await client.GetAlbumAsync(_fixture.Settings.TestAlbumId);

        ResultAssert.ValueFailure(albumResult);
    }
}
