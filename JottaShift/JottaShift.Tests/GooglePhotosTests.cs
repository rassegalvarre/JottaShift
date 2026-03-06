using Castle.Core.Logging;
using JottaShift.Core.GooglePhotos;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;

namespace JottaShift.Tests;

[Trait("API", "Google")]
public class GooglePhotosTests
{
    private const string TestAlbumName = "JottaShift.UnitTests";


    [Fact]
    public async Task GetOrCreateAlbum_CreateOrGetsAlbum_WithAlbumName()
    {
        var googlePhotosRepository = new GooglePhotosRepository(
            new Mock<ILogger<GooglePhotosRepository>>().Object,
            new FileSystem());

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

        var googlePhotosRepository = new GooglePhotosRepository(
            new Mock<ILogger<GooglePhotosRepository>>().Object, 
            new FileSystem());

        var uploadedItems = await googlePhotosRepository.UploadImagesToAlbum(images, TestAlbumName);

        Assert.Equal(images.Count, uploadedItems);
    }
}
