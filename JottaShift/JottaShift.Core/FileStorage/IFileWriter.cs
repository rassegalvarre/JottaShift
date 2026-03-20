namespace JottaShift.Core.FileStorage;

public interface IFileWriter : IDisposable
{
    Task WriteAsync(string text);
    Task WriteLineAsync(string line);
}
