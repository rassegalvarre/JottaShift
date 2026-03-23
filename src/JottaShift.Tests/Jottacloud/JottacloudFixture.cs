using JottaShift.Core.FileStorage;
using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Jottacloud;
using JottaShift.Tests.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudFixture : AppSettingsFixture
{
    public JottacloudHttpClient CreateMockJottacloudHttpClient(IHttpClientWrapper? httpClientWrapper = null)
    {
        httpClientWrapper ??= new Mock<IHttpClientWrapper>().Object;

        return new JottacloudHttpClient(
            httpClientWrapper,
            new Mock<ILogger<JottacloudHttpClient>>().Object);
    }

    public async Task<JottacloudRepository> CreateJottacloudRepository(
        IFileStorageService? fileStorage = null,
        IJottacloudHttpClient? jottacloudClient = null)
    {
        fileStorage ??= new Mock<IFileStorageService>().Object;
        jottacloudClient ??= new Mock<IJottacloudHttpClient>().Object;
        var appSettings = await GetAppSettingsAsync();

        return new JottacloudRepository(
            new Mock<ILogger<JottacloudRepository>>().Object,
            fileStorage,
            jottacloudClient,
            appSettings.JottacloudSettings);
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
