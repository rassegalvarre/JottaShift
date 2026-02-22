using JottaShift.Core.GooglePhotos;

namespace JottaShift.Tests;

// TODO: We dont want to actually make requests to Google in most of these tests.
// Test only the connection. How to solve? Create an addition service that mocks the Google Photos API? Or use a library like Moq to mock the service?
public class GooglePhotosTests
{
    private const string TestAlbumName = "JottaShift.UnitTests";


    [Fact]
    public async Task GetOrCreateAlbum_CreateOrGetsAlbum_WithAlbumName()
    {
        var googlePhotosRepository = new GooglePhotosRepository();

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

        var googlePhotosRepository = new GooglePhotosRepository();

        var uploadedItems = await googlePhotosRepository.UploadImagesToAlbum(images, TestAlbumName);

        Assert.Equal(images.Count, uploadedItems);
    }
}
