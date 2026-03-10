using JottaShift.Core;
using JottaShift.Core.FileStorage;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.Jottacloud.Models.Domain;
using Moq;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudRepositoryTests(JottacloudFixture _fixture)
    : IClassFixture<JottacloudFixture>
{
    [Fact]
    public async Task GetAlbumPhotos_AlbumDoesNotExist()
    {
        var jottacloudClient = new Mock<IJottacloudClient>();
        jottacloudClient.Setup(c => c.GetAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(new Core.Result<Album>(false));

        var jottacloudRepository = _fixture.CreateJottacloudRepository(
            jottacloudClient: jottacloudClient.Object);

        string albumId = Guid.NewGuid().ToString();
        var photos = await jottacloudRepository.GetAlbumPhotos(albumId);

        Assert.Empty(photos);
    }

    [Fact]
    public async Task GetAlbumPhotos_FindLocalPathForPhoto()
    {
        string albumId = Guid.NewGuid().ToString();
        string fileName = Path.GetRandomFileName();

        var album = new Album()
        {
            Id = albumId,
            Photos = [
                new Photo()
                {
                    Filename = fileName,
                    CapturedDate = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                }
            ]
        };

        var jottacloudClient = new Mock<IJottacloudClient>();
        jottacloudClient.Setup(c => c.GetAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(new Core.Result<Album>(true, album));

        var fileStorage = new Mock<IFileStorage>();
        fileStorage.Setup(fs => fs.SearchFileByExactName(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string folder, string fileName, bool recursive) =>
                new Result<string?>(true, Path.Combine(folder, fileName)));

        var jottacloudRepository = _fixture.CreateJottacloudRepository(
            jottacloudClient: jottacloudClient.Object,
            fileStorage: fileStorage.Object);

        var photos = await jottacloudRepository.GetAlbumPhotos(albumId);

        Assert.NotEmpty(photos);
        Assert.NotNull(photos.First().LocalFilePath);
    }

    [Fact]
    public async Task GetAlbumPhotos_PhotoDoesNotExistOnDisk()
    {
        string albumId = Guid.NewGuid().ToString();
        var album = new Album()
        {
            Id = albumId,
            Photos = [
                new Photo()
                {
                    Filename = Path.GetRandomFileName(),
                    CapturedDate = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                }
            ]
        };

        var jottacloudClient = new Mock<IJottacloudClient>();
        jottacloudClient.Setup(c => c.GetAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(new Core.Result<Album>(true, album));

        var fileStorage = new Mock<IFileStorage>();
        fileStorage.Setup(fs => fs.SearchFileByExactName(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new Result<string?>(true));

        var jottacloudRepository = _fixture.CreateJottacloudRepository(
            jottacloudClient: jottacloudClient.Object,
            fileStorage: fileStorage.Object);

        var photos = await jottacloudRepository.GetAlbumPhotos(albumId);

        Assert.NotEmpty(photos);
        Assert.Null(photos.First().LocalFilePath);
    }
}
