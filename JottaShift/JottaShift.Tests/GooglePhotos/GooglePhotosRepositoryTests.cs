using Google.Apis.Auth.OAuth2;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core;
using JottaShift.Tests.TestData;
using Moq;

namespace JottaShift.Tests.GooglePhotos;

public class GooglePhotosRepositoryTests(GooglePhotosFixture _fixture) : IClassFixture<GooglePhotosFixture>
{

    [Fact]
    public async Task GetOrCreateAlbum_ShouldGetExistingAlbum()
    {

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository();

        var album = await googlePhotosRepository.GetOrCreateAlbum(_fixture.TestAlbumName);
        Assert.NotNull(album);
    }

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
