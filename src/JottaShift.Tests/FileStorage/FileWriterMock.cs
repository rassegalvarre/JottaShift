using JottaShift.Core.FileStorage;

namespace JottaShift.Tests.FileStorage;

public class FileWriterMock : IFileWriter
{
    private readonly List<string> _lines = [];

    public FileWriterMock(string fileFullPath)
    {
        _ = fileFullPath;
    }

    public void Dispose()
    {
        _lines.Clear();
        GC.SuppressFinalize(this);
    }

    public Task WriteAsync(string text)
    {
        _lines.Add(text);
        return Task.CompletedTask;
    }

    public Task WriteLineAsync(string line)
    {
        _lines.Add(line);
        return Task.CompletedTask;
    }
}
