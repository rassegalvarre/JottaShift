using JottaShift.Core;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.HttpClientWrapper;
using Moq;
using System.Net;

namespace JottaShift.Tests.GooglePhotos;

public class GooglePhotosHttpClientTests(GooglePhotosFixture _fixture) : IClassFixture<GooglePhotosFixture>
{
    [Fact]
    public async Task UploadPhotoAsync_ShouldReturnFailedResult_WhenInvalidFileNameAndContent()
    {
        var mockFileStorage = new Mock<IFileStorageService>();
        mockFileStorage.Setup(m => m.GetFileName(It.IsAny<string>()))
            .Returns(Result<string>.Failure("Invalid name"));

        mockFileStorage.Setup(m => m.GetFileContent(It.IsAny<string>()))
            .ReturnsAsync(Result<byte[]>.Failure("Invalid data"));

        var googlePhotosHttpClient = _fixture.CreateGooglePhotosHttpClient(
            fileStorage: mockFileStorage.Object);

        var uploadPhotoResult = await googlePhotosHttpClient.UploadPhotoAsync(_fixture.ValidPhotoFullPath);

        Assert.False(uploadPhotoResult.Succeeded);
        Assert.Null(uploadPhotoResult.Value);
        Assert.NotNull(uploadPhotoResult.ErrorMessage);
    }

    [Fact]
    public async Task UploadPhotoAsync_ShouldReturnFailedResult_WhenFailedToGetUserCredentials()
    {
        var mockFileStorage = new Mock<IFileStorageService>();
        mockFileStorage.Setup(m => m.GetFileName(It.IsAny<string>()))
            .Returns(Result<string>.Success(_fixture.ValidPhotoFileName));

        mockFileStorage.Setup(m => m.GetFileContent(It.IsAny<string>()))
            .ReturnsAsync(Result<byte[]>.Success([]));

        var mockUserCredentialManager = new Mock<IUserCredentialManager>();
        mockUserCredentialManager.Setup(m => m.GetAccessTokenAsync())
            .ReturnsAsync(Result<string>.Failure("Failed to get credentials"));

        var googlePhotosHttpClient = _fixture.CreateGooglePhotosHttpClient(
            fileStorage: mockFileStorage.Object,
            userCredentialManager: mockUserCredentialManager.Object);

        var uploadPhotoResult = await googlePhotosHttpClient.UploadPhotoAsync(_fixture.ValidPhotoFullPath);

        Assert.False(uploadPhotoResult.Succeeded);
        Assert.Null(uploadPhotoResult.Value);
        Assert.NotNull(uploadPhotoResult.ErrorMessage);
    }

    [Fact]
    public async Task UploadPhotoAsync_ShouldReturnFailedResult_WhenUploadFailed()
    {

        var mockFileStorage = new Mock<IFileStorageService>();
        mockFileStorage.Setup(m => m.GetFileName(It.IsAny<string>()))
            .Returns(Result<string>.Success(_fixture.ValidPhotoFileName));

        mockFileStorage.Setup(m => m.GetFileContent(It.IsAny<string>()))
            .ReturnsAsync(Result<byte[]>.Success([]));

        var mockUserCredentialManager = new Mock<IUserCredentialManager>();
        mockUserCredentialManager.Setup(m => m.GetAccessTokenAsync())
            .ReturnsAsync(Result<string>.Success(Guid.NewGuid().ToString()));

        var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
        mockHttpClientWrapper.Setup(client => client.SendAsync<string>(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(new HttpSendResult<string>(HttpStatusCode.Forbidden, errorMessage: "Upload failed"));

        var googlePhotosHttpClient = _fixture.CreateGooglePhotosHttpClient(
            fileStorage: mockFileStorage.Object,
            userCredentialManager: mockUserCredentialManager.Object,
            httpClientWrapper: mockHttpClientWrapper.Object);

        var uploadPhotoResult = await googlePhotosHttpClient.UploadPhotoAsync(_fixture.ValidPhotoFullPath);

        Assert.False(uploadPhotoResult.Succeeded);
        Assert.Null(uploadPhotoResult.Value);
        Assert.NotNull(uploadPhotoResult.ErrorMessage);
    }

    [Fact]
    public async Task UploadPhotoAsync_ShouldReturnUploadToken_WhenUploadSucceeded()
    {
        var mockFileStorage = new Mock<IFileStorageService>();
        mockFileStorage.Setup(m => m.GetFileName(It.IsAny<string>()))
            .Returns(Result<string>.Success(_fixture.ValidPhotoFileName));

        mockFileStorage.Setup(m => m.GetFileContent(It.IsAny<string>()))
            .ReturnsAsync(Result<byte[]>.Success([]));

        var mockUserCredentialManager = new Mock<IUserCredentialManager>();
        mockUserCredentialManager.Setup(m => m.GetAccessTokenAsync())
            .ReturnsAsync(Result<string>.Success(Guid.NewGuid().ToString()));

        string expectedUploadToken = Guid.NewGuid().ToString();

        var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
        mockHttpClientWrapper
            .Setup(client => client.SendAsync<string>(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(new HttpSendResult<string>(HttpStatusCode.OK, data: expectedUploadToken));

        var googlePhotosHttpClient = _fixture.CreateGooglePhotosHttpClient(
            fileStorage: mockFileStorage.Object,
            userCredentialManager: mockUserCredentialManager.Object,
            httpClientWrapper: mockHttpClientWrapper.Object);

        var uploadPhotoResult = await googlePhotosHttpClient.UploadPhotoAsync(_fixture.ValidPhotoFullPath);

        Assert.True(uploadPhotoResult.Succeeded);
        Assert.Equal(expectedUploadToken, uploadPhotoResult.Value);
        Assert.Null(uploadPhotoResult.ErrorMessage);
    }
}
