using JottaShift.Core.FileStorage;
using Moq;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudRepositoryTests(JottacloudFixture _fixture)
    : IClassFixture<JottacloudFixture>
{
    public static List<object[]> ImageNameAndFullFilePath() => new()
    {
        new[] { "img_20100612_163249", Path.Combine(JottacloudFixture.Settings.ImageStoragePath, "2010", "6") },
        new[] { "p_20100612_163249", Path.Combine(JottacloudFixture.Settings.ImageStoragePath, "2010", "06") },
        new[] { "v_20100612_163249", Path.Combine(JottacloudFixture.Settings.ImageStoragePath, "2010", "June") },
        new[] { "20100612_163249", Path.Combine(JottacloudFixture.Settings.ImageStoragePath, "2010", "06 - June") }
    };

    [Theory]
    [MemberData(nameof(ImageNameAndFullFilePath))]
    public async Task GetImageFilePathFromFileName_ReturnFilePath_WhenDirectoryMatchesFileName(string imageName, string directoryFullPath)
    {
        var imageDate = new DateTime(2010, 6, 12, 16, 32, 49, DateTimeKind.Local);
        string expectedSearchDirectory = Path.Combine(
            JottacloudFixture.Settings.ImageStoragePath,
            "2010",
            "6");
        string imageFullPath = Path.Combine(directoryFullPath, imageName);

        var fileStorageMock = new Mock<IFileStorage>();
        
        fileStorageMock.Setup(fs => fs.GetImageDate(imageName))
            .Returns(imageDate);

        fileStorageMock.Setup(fs => fs.SearchFileByExactName(expectedSearchDirectory, imageName))
            .Returns(imageFullPath);

        var repository = _fixture.CreateJottacloudRepository(
            fileStorage: fileStorageMock.Object);

        string filePath = await repository.GetImageFilePathFromFileName(imageName);
        string expectedFilePath = Path.Combine(directoryFullPath, imageName);

        Assert.Equal(imageFullPath, filePath);
    }

    [Fact]
    [Trait("API", "Jottacloud")]
    public async Task GetImagesInAlbumAsync_ReturnsAllImages()
    {
        var repository = _fixture.CreateJottacloudRepository();

        var images = await repository.GetImagesInAlbumAsync(JottacloudFixture.Settings.TestAlbumId);

        Assert.NotEmpty(images);
    }
}
