namespace JottaShift.Tests.FileStorage;

public class FileStorageFixture : IDisposable
{
    public readonly string BaseDirectory = @"C:\FileStorage"; 

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
