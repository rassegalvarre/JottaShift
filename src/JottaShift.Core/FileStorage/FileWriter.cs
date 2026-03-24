namespace JottaShift.Core.FileStorage;

public class FileWriter : IFileWriter
{
    private readonly StreamWriter _streamWriter;

    public FileWriter(string fileFullPath)
    {
        _streamWriter = new StreamWriter(fileFullPath);
    }

    public void Dispose()
    {
        _streamWriter?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task WriteAsync(string text)
    {
        await _streamWriter.WriteAsync(text);
    }

    public async Task WriteLineAsync(string line)
    {
        await _streamWriter.WriteLineAsync(line);
    }
}