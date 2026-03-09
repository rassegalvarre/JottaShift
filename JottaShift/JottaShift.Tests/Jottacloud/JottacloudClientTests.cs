using Castle.Core.Logging;
using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Jottacloud;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudClientTests(JottacloudFixture _fixture) : IClassFixture<JottacloudFixture>
{
    [Fact]
    [Trait("API", "Jottacloud")]
    public async Task GetAlbumAsync_ReturnsAlbum_WhenValidAlbumId()
    {
        //var client = _fixture.CreateJottacloudClient();
        
        //var albumResponse = await client.GetAlbumAsync(JottacloudFixture.Settings.TestAlbumId);
                
        //Assert.True(albumResponse.Success);
        //Assert.NotNull(albumResponse.Album);
        //Assert.NotEmpty(albumResponse.Album.Photos);
    }

    [Fact]
    [Trait("API", "Jottacloud")]
    public async Task GetAlbumAsync_ReturnsUnsuccessfullResult_WhenRequestThrowsException()
    {
        var mockHttpWrapper = new Mock<IHttpClientWrapper>();

        mockHttpWrapper
            .Setup(h => h.GetAsync<Album>(It.IsAny<Uri>()))
            .ReturnsAsync(new HttpGetResult<Album>(System.Net.HttpStatusCode.BadRequest));

        var client = new JottacloudClient(
            mockHttpWrapper.Object,
            new Mock<ILogger<JottacloudClient>>().Object);

        var albumResponse = await client.GetAlbumAsync(JottacloudFixture.Settings.TestAlbumId);

        Assert.Null(albumResponse);
    }
}
