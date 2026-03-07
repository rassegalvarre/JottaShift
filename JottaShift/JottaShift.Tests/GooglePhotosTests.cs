namespace JottaShift.Tests;

[Trait("API", "Google")]
public class GooglePhotosTests(GooglePhotosFixture _fixture) : IClassFixture<GooglePhotosFixture>
{
    private const string TestAlbumName = "JottaShift.UnitTests";


    [Fact]
    public async Task GetOrCreateAlbum_CreateOrGetsAlbum_WithAlbumName()
    {
        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository();

        var album = await googlePhotosRepository.GetOrCreateAlbum(TestAlbumName);
        Assert.NotNull(album);
    }

    [Fact]
    public async Task UploadImage_UploadsTestImage()
    {
        var images = new List<string> {
            TestData.Duck,
            TestData.Waterfall
        };

        var googlePhotosRepository = _fixture.CreateGooglePhotosRepository();
        var uploadedItems = await googlePhotosRepository.UploadImagesToAlbum(images, TestAlbumName);

        Assert.Equal(images.Count, uploadedItems);
    }
}
