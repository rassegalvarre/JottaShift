namespace JottaShift.Core.FileStorage;

public class FileWriter(IFileStorageService _fileStorageService) : IFileWriter
{
    private StreamWriter? _streamWriter;

    public void CreateWriter(string fileFullPath)
    {
        var directoryNameResult = _fileStorageService.GetDirectoryName(fileFullPath);
        if (!directoryNameResult.Succeeded || directoryNameResult.Value is null)
        {
            throw new InvalidOperationException("Invalid file-path");
        }

        if (!_fileStorageService.ValidateDirectory(directoryNameResult.Value).Succeeded)
        {
            throw new InvalidOperationException("Could not validate directory.");
        }

        _streamWriter = new StreamWriter(fileFullPath);
    }

    public void Dispose()
    {
        _streamWriter?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task WriteLineAsync(string line)
    {
        if (_streamWriter == null)
        {
            throw new InvalidOperationException("StreamWriter is not initialized. Call CreateWriter() before writing.");
        }

        await _streamWriter.WriteLineAsync(line);
    }
}