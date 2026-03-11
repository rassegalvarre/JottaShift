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

        Assert.True(albumResult.Succeeded);
        Assert.NotNull(albumResult.Value);
        Assert.Equal(existingAlbum.Id, albumResult.Value.Id);
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

        Assert.True(albumResult.Succeeded);
        Assert.NotNull(albumResult.Value);
        Assert.Equal(newAlbum.Id, albumResult.Value.Id);
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

        Assert.False(albumResult.Succeeded);
        Assert.Null(albumResult.Value);
    }
    #endregion

    #region

    //[Fact(Skip = "Must create an abstraction for GooglePhotosService")]
    //public async Task UploadImage_UploadsTestImage()
    //{
    //    var images = new List<string> {
    //        TestDataHelper.Duck,
    //        TestDataHelper.Waterfall
    //    };

    //    var googlePhotosClient = new Mock<IGooglePhotosHttpClient>();
    //    googlePhotosClient.Setup(c => c.UploadPhoto(
    //        It.IsAny<UserCredential>(),
    //        It.IsAny<string>(),
    //        It.IsAny<byte[]>()))
    //        .ReturnsAsync(Result<string>.Success(Guid.NewGuid().ToString()));

    //    var googlePhotosRepository = _fixture.CreateGooglePhotosRepository(googlePhotosClient.Object);
    //    var uploadedItems = await googlePhotosRepository.UploadPhotosToAlbum(images, TestAlbumName);

    //    Assert.Equal(images.Count, uploadedItems);
    //}
}
