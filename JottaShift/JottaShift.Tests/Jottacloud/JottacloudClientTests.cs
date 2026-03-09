using Castle.Core.Logging;
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
        var client = _fixture.CreateJottacloudClient();
        
        var albumResponse = await client.GetAlbumAsync(JottacloudFixture.Settings.TestAlbumId);
                
        Assert.True(albumResponse.Success);
        Assert.NotNull(albumResponse.Album);
        Assert.NotEmpty(albumResponse.Album.Photos);
    }

    [Fact]
    [Trait("API", "Jottacloud")]
    public async Task GetAlbumAsync_ReturnsUnsuccessfullResult_WhenRequestThrowsException()
    {
        var mockMessageHandler = new Mock<HttpMessageHandler>();

        mockMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockMessageHandler.Object);


        var client = new JottacloudClient(
            httpClient,
            new Mock<ILogger<JottacloudClient>>().Object);

        var albumResponse = await client.GetAlbumAsync(JottacloudFixture.Settings.TestAlbumId);

        Assert.False(albumResponse.Success);
        Assert.Null(albumResponse.Album);
    }
}
