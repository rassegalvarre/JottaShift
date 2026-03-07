using JottaShift.Core.FileStorage;
using JottaShift.Core.Jottacloud;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudFixture : IDisposable
{
    public JottacloudRepository CreateJottacloudRepository(
        IFileStorage? fileStorage = null)
    {
        fileStorage ??= new Mock<IFileStorage>().Object;

        return new JottacloudRepository(
            new Mock<ILogger<JottacloudRepository>>().Object,
            fileStorage);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
