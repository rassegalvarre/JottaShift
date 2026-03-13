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

    // TODO: Reanme to CreateMockJottacloudHttpClient
    public JottacloudHttpClient CreateJottacloudClient(IHttpClientWrapper? httpClientWrapper = null)
    {
        httpClientWrapper ??= new Mock<IHttpClientWrapper>().Object;

        return new JottacloudHttpClient(
            httpClientWrapper,
            new Mock<ILogger<JottacloudHttpClient>>().Object);
    }

    public JottacloudRepository CreateJottacloudRepository(
        IFileStorageService? fileStorage = null,
        IJottacloudHttpClient? jottacloudClient = null)
    {
        fileStorage ??= new Mock<IFileStorageService>().Object;
        jottacloudClient ??= new Mock<IJottacloudHttpClient>().Object;

        return new JottacloudRepository(
            new Mock<ILogger<JottacloudRepository>>().Object,
            fileStorage,
            jottacloudClient,
            Settings);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
