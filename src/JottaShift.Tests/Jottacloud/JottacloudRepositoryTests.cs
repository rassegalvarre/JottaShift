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
    public async Task GetAlbumAsync_AlbumDoesNotExist()
    {
        var jottacloudClient = new Mock<IJottacloudHttpClient>();
        jottacloudClient.Setup(c => c.GetAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Failure("Album not found"));

        var jottacloudRepository = await _fixture.CreateJottacloudRepository(
            jottacloudClient: jottacloudClient.Object);

        string albumId = Guid.NewGuid().ToString();
        var albumResult = await jottacloudRepository.GetAlbumAsync(albumId);

        ResultAssert.ValueFailure(albumResult);
    }

    [Fact]
    public async Task GetAlbumAsync_FindLocalPathForPhoto()
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

        var jottacloudClient = new Mock<IJottacloudHttpClient>();
        jottacloudClient.Setup(c => c.GetAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Success(album));

        var fileStorage = new Mock<IFileStorageService>();
        fileStorage.Setup(fs => fs.SearchFileByExactName(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string folder, string fileName, bool recursive) =>
                Result<string>.Success(Path.Combine(folder, fileName)));

        var jottacloudRepository = await _fixture.CreateJottacloudRepository(
            jottacloudClient: jottacloudClient.Object,
            fileStorage: fileStorage.Object);

        var albumResult = await jottacloudRepository.GetAlbumAsync(albumId);

        ResultAssert.Success(albumResult);
        Assert.NotEmpty(albumResult.Value!.Photos);
        Assert.NotNull(albumResult.Value.Photos.First().LocalFilePath);
    }

    [Fact]
    public async Task GetAlbumAsync_PhotoDoesNotExistOnDisk()
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

        var jottacloudClient = new Mock<IJottacloudHttpClient>();
        jottacloudClient.Setup(c => c.GetAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<Album>.Success(album));

        var fileStorage = new Mock<IFileStorageService>();
        fileStorage.Setup(fs => fs.SearchFileByExactName(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Result<string>.Failure("Not found"));

        var jottacloudRepository = await _fixture.CreateJottacloudRepository(
            jottacloudClient: jottacloudClient.Object,
            fileStorage: fileStorage.Object);

        var albumResult = await jottacloudRepository.GetAlbumAsync(albumId);

        ResultAssert.Success(albumResult);
        Assert.NotEmpty(albumResult.Value!.Photos);
        Assert.Null(albumResult.Value.Photos.First().LocalFilePath);
    }
}
