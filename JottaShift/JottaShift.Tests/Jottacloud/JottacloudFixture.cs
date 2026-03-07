namespace JottaShift.Tests.Jottacloud;

public class JottacloudFixture : IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
