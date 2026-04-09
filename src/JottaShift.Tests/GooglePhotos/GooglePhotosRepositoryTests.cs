using Google.Apis.PhotosLibrary.v1.Data;
using JottaShift.Core;
using JottaShift.Core.GooglePhotos;
using Moq;

namespace JottaShift.Tests.GooglePhotos;

public class GooglePhotosRepositoryTests(GooglePhotosFixture _fixture) : IClassFixture<GooglePhotosFixture>
{
    #region GetOrCreateAlbum
    [Fact]
    public async Task GetOrCreateAlbum_ShouldGetExistingAlbum()
    {
        Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosLibraryFacade = new Mock<IGooglePhotosLibraryFacade>();
        mockGooglePhotosLibraryFacade.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Success(existingAlbum));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosLibraryFacade.Object);

        var albumResult = await googlePhotosRepository.GetOrCreateAlbumAsync(_fixture.TestAlbumName);

        ResultAssert.ValueSuccess(albumResult, existingAlbum);
    }

    [Fact]
    public async Task GetOrCreateAlbum_ShouldCreateNewWhenNoneExist()
    {
        Album newAlbum = new()
        {
            Id = "new-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosLibraryFacade = new Mock<IGooglePhotosLibraryFacade>();
        mockGooglePhotosLibraryFacade.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Failure("Not found"));

        mockGooglePhotosLibraryFacade.Setup(f => f.CreateAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Success(newAlbum));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosLibraryFacade.Object);

        var albumResult = await googlePhotosRepository.GetOrCreateAlbumAsync(_fixture.TestAlbumName);

        ResultAssert.ValueSuccess(albumResult, newAlbum);
    }

    [Fact]
    public async Task GetOrCreateAlbum_ShouldReturnFailure_WhenUnableToCreateNewAlbum()
    {
        var mockGooglePhotosLibraryFacade = new Mock<IGooglePhotosLibraryFacade>();
        mockGooglePhotosLibraryFacade.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Failure("Not found"));

        mockGooglePhotosLibraryFacade.Setup(f => f.CreateAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Failure("Cant create"));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosLibraryFacade.Object);

        var albumResult = await googlePhotosRepository.GetOrCreateAlbumAsync(_fixture.TestAlbumName);

        ResultAssert.ValueFailure(albumResult);
    }
    #endregion

    #region UploadPhotosToAlbum
    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnFailure_WhenInvalidAlbum()
    {
        var mockGooglePhotosLibraryFacade = new Mock<IGooglePhotosLibraryFacade>();
        mockGooglePhotosLibraryFacade.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Failure("Not found"));

        mockGooglePhotosLibraryFacade.Setup(f => f.CreateAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Failure("Not created"));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosLibraryFacade.Object);

        List<string> filePaths = [ _fixture.ValidPhotoFullPath ];

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);

        AlbumUploadResultAssert.Failure(uploadResult);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnFailure_WhenUnableToGetToken()
    {
        Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosLibraryFacade = new Mock<IGooglePhotosLibraryFacade>();
        mockGooglePhotosLibraryFacade.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Success(existingAlbum));

        var mockGooglePhotosClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosClient.Setup(f => f.UploadMediaAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Failure("Token failure"));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(
            mockGooglePhotosLibraryFacade.Object,
            mockGooglePhotosClient.Object);
        
        List<string> filePaths = [_fixture.ValidPhotoFullPath];

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);

        AlbumUploadResultAssert.Failure(uploadResult);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnFailure_WhenUnableToAddPhotosToAlbum()
    {
        Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosLibraryFacade = new Mock<IGooglePhotosLibraryFacade>();
        mockGooglePhotosLibraryFacade.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Success(existingAlbum));

        mockGooglePhotosLibraryFacade.Setup(f => f.AddImagesToAlbum(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Result<BatchCreateMediaItemsResponse>.Failure("Cant add to album"));

        var mockGooglePhotosClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosClient.Setup(f => f.UploadMediaAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Success(Guid.NewGuid().ToString()));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(
            mockGooglePhotosLibraryFacade.Object,
            mockGooglePhotosClient.Object);

        List<string> filePaths = [_fixture.ValidPhotoFullPath];

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);

        AlbumUploadResultAssert.Failure(uploadResult);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnSuccess_WhenNoFilesProvided()
    {
        Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        var mockGooglePhotosLibraryFacade = new Mock<IGooglePhotosLibraryFacade>();
        mockGooglePhotosLibraryFacade.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Success(existingAlbum));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(mockGooglePhotosLibraryFacade.Object);

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, []);

        AlbumUploadResultAssert.Success(uploadResult);
        Assert.Empty(uploadResult.PhotoUploadResults);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnFailure_WhenNoPhotosUploaded()
    {
        Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        BatchCreateMediaItemsResponse batchCreateResponse = new()
        {
            NewMediaItemResults = []
        };

        var mockGooglePhotosLibraryFacade = new Mock<IGooglePhotosLibraryFacade>();
        mockGooglePhotosLibraryFacade.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Success(existingAlbum));

        var mockGooglePhotosClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosClient.Setup(f => f.UploadMediaAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Success(Guid.NewGuid().ToString()));

        mockGooglePhotosLibraryFacade.Setup(f => f.AddImagesToAlbum(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Result<BatchCreateMediaItemsResponse>.Success(batchCreateResponse));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(
            mockGooglePhotosLibraryFacade.Object,
            mockGooglePhotosClient.Object);

        List<string> filePaths = [ _fixture.ValidPhotoFullPath ];

        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);

        AlbumUploadResultAssert.Failure(uploadResult);
    }

    [Fact]
    public async Task UploadPhotosToAlbumAsync_ShouldReturnSuccess_WhenPhotosWereUploaded()
    {
        List<string> filePaths = [_fixture.ValidPhotoFullPath];
        string uploadToken = Guid.NewGuid().ToString();
        
        Album existingAlbum = new()
        {
            Id = "existing-album-id",
            Title = _fixture.TestAlbumName
        };

        BatchCreateMediaItemsResponse batchCreateResponse = new()
        {
            NewMediaItemResults = [.. filePaths.Select(f => new NewMediaItemResult()
            {
                MediaItem = new MediaItem()
                {
                    Id = f
                },
                UploadToken = uploadToken,
                Status = new Status()
                {
                    Message = "Success"
                }
            })]
        };

        var mockGooglePhotosLibraryFacade = new Mock<IGooglePhotosLibraryFacade>();
        mockGooglePhotosLibraryFacade.Setup(f => f.GetAlbumFromTitleAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Success(existingAlbum));

        var mockGooglePhotosClient = new Mock<IGooglePhotosHttpClient>();
        mockGooglePhotosClient.Setup(f => f.UploadMediaAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Success(uploadToken));

        mockGooglePhotosLibraryFacade.Setup(f => f.AddImagesToAlbum(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Result<BatchCreateMediaItemsResponse>.Success(batchCreateResponse));

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(
            mockGooglePhotosLibraryFacade.Object,
            mockGooglePhotosClient.Object);


        var uploadResult = await googlePhotosRepository.UploadPhotosToAlbumAsync(_fixture.TestAlbumName, filePaths);
        var uploadedPhoto = uploadResult.PhotoUploadResults.First();

        AlbumUploadResultAssert.Success(uploadResult);

        Assert.Equal(filePaths.First(), uploadedPhoto.Id);
        Assert.Equal(uploadToken, uploadedPhoto.UploadToken);
        Assert.Equal("Success", uploadedPhoto.StatusMessage);
    }
    #endregion
}
