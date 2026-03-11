using JottaShift.Core;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.HttpClientWrapper;
using Moq;
using System.Net;

namespace JottaShift.Tests.GooglePhotos;

public class GooglePhotosHttpClientTests(GooglePhotosFixture _fixture) : IClassFixture<GooglePhotosFixture>
{
    [Fact]
    public async Task UploadPhot_ShouldReturnFailedResult_WhenFailedToGetUserCredentials()
    {
        var mockUserCredentialManager = new Mock<IUserCredentialManager>();
        mockUserCredentialManager.Setup(m => m.GetAccessTokenAsync())
            .ReturnsAsync(Result<string>.Failure("Failed to get crentials"));

        var googlePhotosHttpClient = _fixture.CreateGooglePhotosHttpClient(
            userCredentialManager: mockUserCredentialManager.Object);

        var uploadPhotoResult = await googlePhotosHttpClient.UploadPhotoAsync("test.jpg", [0x01, 0x02]);

        Assert.False(uploadPhotoResult.Succeeded);
        Assert.Null(uploadPhotoResult.Value);
        Assert.NotNull(uploadPhotoResult.ErrorMessage);
    }

    [Fact]
    public async Task UploadPhot_ShouldReturnFailedResult_WhenUploadFailed()
    {        
        var mockUserCredentialManager = new Mock<IUserCredentialManager>();
        mockUserCredentialManager
            .Setup(m => m.GetAccessTokenAsync())
            .ReturnsAsync(Result<string>.Success(Guid.NewGuid().ToString()));

        var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
        mockHttpClientWrapper
            .Setup(client => client.SendAsync<string>(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(new HttpSendResult<string>(HttpStatusCode.Forbidden, "Upload failed"));

        var googlePhotosHttpClient = _fixture.CreateGooglePhotosHttpClient(
            userCredentialManager: mockUserCredentialManager.Object,
            httpClientWrapper: mockHttpClientWrapper.Object);

        var uploadPhotoResult = await googlePhotosHttpClient.UploadPhotoAsync("test.jpg", [0x01, 0x02]);

        Assert.False(uploadPhotoResult.Succeeded);
        Assert.Null(uploadPhotoResult.Value);
        Assert.NotNull(uploadPhotoResult.ErrorMessage);
    }
}
