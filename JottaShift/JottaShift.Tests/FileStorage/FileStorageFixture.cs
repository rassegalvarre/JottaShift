namespace JottaShift.Tests.FileStorage;

public class FileStorageFixture : IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
