namespace JottaShift.Tests.Jottacloud;

public class JottacloudRepositoryTests(JottacloudFixture _fixture)
    : IClassFixture<JottacloudFixture>
{
    [Fact]
    public async Task GetAlbumImages_AlbumDoesNotExist()
    {

    }

    [Fact]
    public async Task GetAlbumImages_FindLocalPathForPhoto()
    {
       
    }

    [Fact]
    public async Task GetAlbumImages_PhotoDoesNotExistOnDisk()
    {

    }
}
