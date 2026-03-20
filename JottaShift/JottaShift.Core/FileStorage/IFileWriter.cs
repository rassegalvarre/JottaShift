namespace JottaShift.Core.FileStorage;

public interface IFileWriter : IDisposable
{
    void CreateWriter(string fileFullPath);
    Task WriteLineAsync(string line);
}
