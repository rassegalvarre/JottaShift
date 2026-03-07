namespace JottaShift.Tests.Jottacloud;

public class JottacloudRepositoryTests(JottacloudFixture _fixture)
    : IClassFixture<JottacloudFixture>
{
    public const string BaseDirectory = @"C:\Jottacloud";

    public static List<object[]> ImageNameAndFullFilePath() => new()
    {
        new[] { "img_20100612_163249", Path.Combine(BaseDirectory, "Images", "2010", "6") },
        new[] { "p_20100612_163249", Path.Combine(BaseDirectory, "Images", "2010", "06") },
        new[] { "v_20100612_163249", Path.Combine(BaseDirectory, "Images", "2012", "June") },
        new[] { "20100612_163249", Path.Combine(BaseDirectory, "Images", "2012", "06 - June") }
    };


    [Theory]
    [MemberData(nameof(ImageNameAndFullFilePath))]
    public async Task GetImageFilePathFromFileName_ReturnFilePath_WhenDirectoryMatchesFileName(string imageName, string directoryFullPath)
    {
        var repository = _fixture.CreateJottacloudRepository();

        string filePath = await repository.GetImageFilePathFromFileName(imageName);
        string expectedFilePath = Path.Combine(directoryFullPath, imageName);

        Assert.Equal(expectedFilePath, filePath);
    }
}
