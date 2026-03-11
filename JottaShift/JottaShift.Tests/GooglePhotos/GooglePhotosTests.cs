using Google.Apis.Auth.OAuth2;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core;
using JottaShift.Tests.TestData;
using Moq;

namespace JottaShift.Tests.GooglePhotos;

[Trait("API", "Google")]
public class GooglePhotosTests(GooglePhotosFixture _fixture) : IClassFixture<GooglePhotosFixture>
{
    private const string TestAlbumName = "JottaShift.UnitTests";


    //[Fact]
    //public async Task GetOrCreateAlbum_CreateOrGetsAlbum_WithAlbumName()
    //{
    //    var googlePhotosRepository = _fixture.CreateGooglePhotosRepository();

    //    var album = await googlePhotosRepository.GetOrCreateAlbum(TestAlbumName);
    //    Assert.NotNull(album);
    //}

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
