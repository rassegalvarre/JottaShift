using JottaShift.Core.GooglePhotos;
using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Tests;

// TODO: We dont want to actually make requests to Google in most of these tests.
// Test only the connection. How to solve? Create an addition service that mocks the Google Photos API? Or use a library like Moq to mock the service?
public class GooglePhotosTests
{
    [Fact]
    public void GetPhotosLibraryService_CreatesService_WhenCredentialsFound()
    {
        var googlePhotosRepository = new GooglePhotosRepository();
        var credential = googlePhotosRepository.Credential();

        var photosLibraryService = googlePhotosRepository.GetPhotosLibraryService(credential);

        Assert.NotNull(photosLibraryService);
    }

    [Fact]
    public async Task CreateAlbum_CreatesAnAlbum()
    {
        var googlePhotosRepository = new GooglePhotosRepository();

        var result = await googlePhotosRepository.CreateAlbum();

        Assert.True(result);
    }

    [Fact]
    public async Task UploadImage_UploadsTestImage()
    {
        var googlePhotosRepository = new GooglePhotosRepository();

        var result = await googlePhotosRepository.UploadImage();

        Assert.True(result);
    }
}
