namespace JottaShift.Tests;

public class JottacloudFixture : IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
