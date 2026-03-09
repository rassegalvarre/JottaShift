using JottaShift.Core.FileStorage;
using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Jottacloud;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudFixture : IDisposable
{
    public JottacloudSettings Settings => new()
    {
        ApiUri = "https://api.jottacloud.com/",
        SyncFolderFullPath = @"C:\\Jottacloud",
        PhotoStoragePath = @"C:\\Jottacloud\\Images",
        TestAlbumId = "imjg7a52t61g"
    };

    public JottacloudClient CreateJottacloudClient(IHttpClientWrapper? httpClientWrapper = null)
    {
        httpClientWrapper ??= new Mock<IHttpClientWrapper>().Object;

        return new JottacloudClient(
            httpClientWrapper,
            new Mock<ILogger<JottacloudClient>>().Object);
    }

    public JottacloudRepository CreateJottacloudRepository(
        IFileStorage? fileStorage = null)
    {
        fileStorage ??= new Mock<IFileStorage>().Object;

        return new JottacloudRepository(
            new Mock<ILogger<JottacloudRepository>>().Object,
            fileStorage,
            new Mock<JottacloudClient>().Object,
            Settings);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
