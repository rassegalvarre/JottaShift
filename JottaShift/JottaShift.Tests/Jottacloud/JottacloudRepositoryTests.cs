using JottaShift.Core.FileStorage;
using Moq;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudRepositoryTests(JottacloudFixture _fixture)
    : IClassFixture<JottacloudFixture>
{
    public const string BaseDirectory = @"C:\Jottacloud";

    public static List<object[]> ImageNameAndFullFilePath() => new()
    {
        new[] { "img_20100612_163249", Path.Combine(BaseDirectory, "Images", "2010", "6") },
        new[] { "p_20100612_163249", Path.Combine(BaseDirectory, "Images", "2010", "06") },
        new[] { "v_20100612_163249", Path.Combine(BaseDirectory, "Images", "2010", "June") },
        new[] { "20100612_163249", Path.Combine(BaseDirectory, "Images", "2010", "06 - June") }
    };

    [Theory]
    [MemberData(nameof(ImageNameAndFullFilePath))]
    public async Task GetImageFilePathFromFileName_ReturnFilePath_WhenDirectoryMatchesFileName(string imageName, string directoryFullPath)
    {
        var imageDate = new DateTime(2010, 6, 12, 16, 32, 49, DateTimeKind.Local);
        var imageFullPath = Path.Combine(directoryFullPath, imageName);

        var fileStorageMock = new Mock<IFileStorage>();
        
        fileStorageMock.Setup(fs => fs.GetImageDate(It.IsAny<string>()))
            .Returns(imageDate);

        fileStorageMock.Setup(fs => fs.SearchFileByExactName(
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(imageFullPath);

        var repository = _fixture.CreateJottacloudRepository(
            fileStorage: fileStorageMock.Object);

        string filePath = await repository.GetImageFilePathFromFileName(imageName);
        string expectedFilePath = Path.Combine(directoryFullPath, imageName);

        Assert.Equal(expectedFilePath, filePath);
    }
}
