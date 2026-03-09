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
    public async Task GetAlbumAsync_WithImplementedHttpClient_ReturnsAlbum_WhenValidAlbumIdUsingHttpClient()
    {
        var httpClientWrapper = _httpClientFixture.CreateHttpClientWrapper();
        var client = _fixture.CreateJottacloudClient(httpClientWrapper);
        
        var albumResult = await client.GetAlbumAsync(JottacloudFixture.Settings.TestAlbumId);

        Assert.True(albumResult.Success);
        Assert.NotNull(albumResult.Value);
        Assert.NotEmpty(albumResult.Value.Photos);
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

        var albumResult = await client.GetAlbumAsync(JottacloudFixture.Settings.TestAlbumId);

        Assert.True(albumResult.Success);
        Assert.NotNull(albumResult.Value);
        Assert.Equal(expectedAlbumResponse.Id, albumResult.Value.Id);
        Assert.Equal(expectedPhoto, expectedAlbumResponse.Photos.Single());
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


        var albumResult = await client.GetAlbumAsync(JottacloudFixture.Settings.TestAlbumId);

        Assert.False(albumResult.Success);
        Assert.Null(albumResult.Value);
    }
}
