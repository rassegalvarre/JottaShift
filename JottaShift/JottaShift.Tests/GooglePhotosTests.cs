using JottaShift.Core.GooglePhotos;
using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Tests;

// TODO: We dont want to actually make requests to Google in most of these tests.
// Test only the connection. How to solve? Create an addition service that mocks the Google Photos API? Or use a library like Moq to mock the service?
public class GooglePhotosTests
{
    // TODO: Move to TestData-class
    private static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string Duck = Path.Combine(TestDataPath, "duck.jpg");

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
        var images = new List<string> { Duck };

        var googlePhotosRepository = new GooglePhotosRepository();

        var result = await googlePhotosRepository.UploadImageToAlbum(images, GooglePhotosRepository.AlbumName);

        Assert.True(result);
    }
}
