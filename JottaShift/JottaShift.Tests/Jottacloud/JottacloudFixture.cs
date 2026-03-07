using JottaShift.Core.FileStorage;
using JottaShift.Core.Jottacloud;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudFixture : IDisposable
{
    public static JottacloudSettings Settings => new()
    {
        SyncFolderFullPath = @"C:\\Jottacloud",
        ImageStoragePath = @"C:\\Jottacloud\\Images"
    };

    public JottacloudRepository CreateJottacloudRepository(
        IFileStorage? fileStorage = null)
    {
        fileStorage ??= new Mock<IFileStorage>().Object;

        return new JottacloudRepository(
            new Mock<ILogger<JottacloudRepository>>().Object,
            fileStorage,
            Settings);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
