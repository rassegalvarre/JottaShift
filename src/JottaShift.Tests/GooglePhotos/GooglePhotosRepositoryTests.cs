using JottaShift.Core;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.GooglePhotos.Models.PhotosLibraryV1Data;
using Moq;

namespace JottaShift.Tests.GooglePhotos;

public class GooglePhotosRepositoryTests(GooglePhotosFixture _fixture) : IClassFixture<GooglePhotosFixture>
{
    #region GetOrCreateAlbum
    [Fact]
    public async Task GetOrCreateAlbum_ShouldGetExistingAlbum()
    {
        JS_Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosHttpClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosHttpClient.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Success(existingAlbum));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosHttpClient.Object);
        var albumResult = await googlePhotosRepository.GetOrCreateAlbumAsync(_fixture.TestAlbumName);

        ResultAssert.ValueSuccess(albumResult, existingAlbum);
    }

    [Fact]
    public async Task GetOrCreateAlbum_ShouldCreateNewWhenNoneExist()
    {
        JS_Album newAlbum = new()
        {
            Id = "new-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosHttpClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosHttpClient.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Failure("Not found"));

        mockGooglePhotosHttpClient.Setup(f => f.CreateAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Success(newAlbum));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosHttpClient.Object);

        var albumResult = await googlePhotosRepository.GetOrCreateAlbumAsync(_fixture.TestAlbumName);

        ResultAssert.ValueSuccess(albumResult, newAlbum);
    }

    [Fact]
    public async Task GetOrCreateAlbum_ShouldReturnFailure_WhenUnableToCreateNewAlbum()
    {
        var mockGooglePhotosHttpClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosHttpClient.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Failure("Not found"));

        mockGooglePhotosHttpClient.Setup(f => f.CreateAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Failure("Cant create"));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosHttpClient.Object);

        var albumResult = await googlePhotosRepository.GetOrCreateAlbumAsync(_fixture.TestAlbumName);

        ResultAssert.ValueFailure(albumResult);
    }
    #endregion

    #region UploadPhotosToAlbum
    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnFailure_WhenInvalidAlbum()
    {
        var mockGooglePhotosHttpClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosHttpClient.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Failure("Not found"));

        mockGooglePhotosHttpClient.Setup(f => f.CreateAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Failure("Not created"));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosHttpClient.Object);

        List<string> filePaths = [ _fixture.ValidPhotoFullPath ];

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);

        AlbumUploadResultAssert.Failure(uploadResult);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnFailure_WhenUnableToGetToken()
    {
        JS_Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosHttpClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosHttpClient.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Success(existingAlbum));

        mockGooglePhotosHttpClient.Setup(f => f.UploadMediaAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Failure("Token failure"));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(
            mockGooglePhotosHttpClient.Object);
        
        List<string> filePaths = [_fixture.ValidPhotoFullPath];

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);

        AlbumUploadResultAssert.Failure(uploadResult);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnFailure_WhenUnableToAddPhotosToAlbum()
    {
        JS_Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosHttpClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosHttpClient.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Success(existingAlbum));

        mockGooglePhotosHttpClient.Setup(f => f.BatchAddMediaItemsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Result<JS_BatchCreateMediaItemsResponse>.Failure("Cant add to album"));

        mockGooglePhotosHttpClient.Setup(f => f.UploadMediaAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Success(Guid.NewGuid().ToString()));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(
            mockGooglePhotosHttpClient.Object);

        List<string> filePaths = [_fixture.ValidPhotoFullPath];

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);

        AlbumUploadResultAssert.Failure(uploadResult);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnSuccess_WhenNoFilesProvided()
    {
        JS_Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosHttpClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosHttpClient.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Success(existingAlbum));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosHttpClient.Object);

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, []);

        AlbumUploadResultAssert.Success(uploadResult);
        Assert.Empty(uploadResult.PhotoUploadResults);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnFailure_WhenNoPhotosUploaded()
    {
        JS_Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        JS_BatchCreateMediaItemsResponse batchCreateResponse = new()
        {
            NewMediaItemResults = []
        };

        var mockGooglePhotosHttpClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosHttpClient.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Success(existingAlbum));

        mockGooglePhotosHttpClient.Setup(f => f.UploadMediaAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Success(Guid.NewGuid().ToString()));

        mockGooglePhotosHttpClient.Setup(f => f.BatchAddMediaItemsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Result<JS_BatchCreateMediaItemsResponse>.Success(batchCreateResponse));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(
            mockGooglePhotosHttpClient.Object);

        List<string> filePaths = [ _fixture.ValidPhotoFullPath ];

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);

        AlbumUploadResultAssert.Failure(uploadResult);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnSuccess_WhenPhotosWereUploaded()
    {
        List<string> filePaths = [_fixture.ValidPhotoFullPath];
        string uploadToken = Guid.NewGuid().ToString();
        
        JS_Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        JS_BatchCreateMediaItemsResponse batchCreateResponse = new()
        {
            NewMediaItemResults = [.. filePaths.Select(f => new JS_NewMediaItemResult()
            {
                MediaItem = new JS_MediaItem()
                {
                    Id = f
                },
                UploadToken = uploadToken,
                Status = new JS_Status()
                {
                    Message = "Success"
                }
            })]
        };

        var mockGooglePhotosHttpClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosHttpClient.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<JS_Album>.Success(existingAlbum));

        mockGooglePhotosHttpClient.Setup(f => f.UploadMediaAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Success(uploadToken));

        mockGooglePhotosHttpClient.Setup(f => f.BatchAddMediaItemsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Result<JS_BatchCreateMediaItemsResponse>.Success(batchCreateResponse));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(
            mockGooglePhotosHttpClient.Object);

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);
        var uploadedPhoto = uploadResult.PhotoUploadResults.First();

        AlbumUploadResultAssert.Success(uploadResult);

        Assert.Equal(filePaths.First(), uploadedPhoto.Id);
        Assert.Equal(uploadToken, uploadedPhoto.UploadToken);
        Assert.Equal("Success", uploadedPhoto.StatusMessage);
    }
    #endregion
}
