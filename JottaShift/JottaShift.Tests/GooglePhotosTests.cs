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
    public void GetPhotosLibraryService_CreatesService_WhenCredentialsFound()
    {
        var googlePhotosRepository = new GooglePhotosRepository();
        var credential = googlePhotosRepository.CreateUserCredential();

        var photosLibraryService = googlePhotosRepository.GetPhotosLibraryService(credential);

        Assert.NotNull(photosLibraryService);
    }

    [Fact]
    public async Task InitializeAlbum_CreatesAlbum_WhenNoneExists()
    {
        var googlePhotosRepository = new GooglePhotosRepository();

        var noneExstingAlbum = await googlePhotosRepository.GetAlbum(TestAlbumName);
        Assert.Null(noneExstingAlbum);

        var newAbum = await googlePhotosRepository.InitializeAlbum(TestAlbumName);
        Assert.NotNull(newAbum);
    }

    [Fact]
    public async Task InitializeAlbum_ReturnsExistingAlbum_WhenOneExists()
    {
        var googlePhotosRepository = new GooglePhotosRepository();

        var exixstingAlbum = await googlePhotosRepository.GetAlbum(TestAlbumName);
        Assert.NotNull(exixstingAlbum);

        var newResult = await googlePhotosRepository.InitializeAlbum(TestAlbumName);
        Assert.Equal(exixstingAlbum.Id, newResult.Id);
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
