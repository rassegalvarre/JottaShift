using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace JottaShift.Core.FileStorage;

public sealed class FileStorageService(
    IFileSystem _fileSystem,
    ILogger<FileStorageService> _logger) : IFileStorage
{
    public async Task CopyAsync(string sourceFileFullPath, string targetDirectory, bool deleteSource, CancellationToken ct = default)
    {
        if (!_fileSystem.File.Exists(sourceFileFullPath))
        {
            _logger.LogWarning("Source file not found: {FilePath}", sourceFileFullPath);
            return;
        }

        if (!ValidateDirectory(new DirectoryOptions(targetDirectory, true)))
        {
            _logger.LogWarning("Could not find or create target directory: {TargeDirectory}", targetDirectory);
            return;
        }

        try
        {
            string fileName = Path.GetFileName(sourceFileFullPath);
            string newFileName = Path.Combine(targetDirectory, fileName);
            _fileSystem.File.Copy(sourceFileFullPath, newFileName, false);
        }
        catch(Exception ex)
        {
            _logger.LogError("Exception when copying file: {ExceptionMessage}", ex.Message);
        }

        if (deleteSource)
        {
            try
            {
                _fileSystem.File.Delete(sourceFileFullPath);
                _logger.LogInformation("Deleted source file: {SourceFile}", sourceFileFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception when deleting source file: {ExceptionMessage}", ex.Message);
            }
        }

        await Task.FromResult(true);
    }

    public DateTime GetFileTimestamp(string fileFullPath)
    {
        if (!_fileSystem.File.Exists(fileFullPath))
        {
            return default;
        }

        return  _fileSystem.File.GetLastWriteTime(fileFullPath);
    }

    public IEnumerable<string> EnumerateDirectories(string directoryFullPath)
    {
        if (!_fileSystem.Directory.Exists(directoryFullPath))
        {
            return [];
        }

        return _fileSystem.Directory.EnumerateDirectories(directoryFullPath);
    }


    public IEnumerable<string> EnumerateFiles(string directoryFullPath)
    {
        if (!_fileSystem.Directory.Exists(directoryFullPath))
        {
            return [];
        }

        return _fileSystem.Directory.EnumerateFiles(directoryFullPath, "*", SearchOption.AllDirectories);
    }

    public bool ValidateDirectory(DirectoryOptions options)
    {
        bool validated = false;
        if (_fileSystem.Directory.Exists(options.directoryFullPath))
        {
            validated = true;
        }
        else if (options.createIfNotExists)
        {
            try
            {
                var directory = _fileSystem.Directory.CreateDirectory(options.directoryFullPath);
                validated = directory.Exists;
                _logger.LogInformation(
                    "Created directory @{DirectoryFullPath}", options.directoryFullPath);
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    "An exception occured when creating directory @{DirectoryFullPath}. Exception: @{ExceptionMessage}",
                    options.directoryFullPath,
                    ex.Message);
                validated = false;
            }
        }

        return validated;
    }

    public bool FilesAreBitPerfectMatch(string pathA, string pathB, int bufferSize = 1024 * 1024) // 1 MiB default
    {
        // Quick size check – if lengths differ they can’t be identical.
        var infoA = _fileSystem.FileInfo.New(pathA);
        var infoB = _fileSystem.FileInfo.New(pathB);
        if (infoA.Length != infoB.Length)
            return false;

        // Open both streams for sequential reading.
        using var fsA = _fileSystem.FileStream.New(pathA, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var fsB = _fileSystem.FileStream.New(pathB, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bufferA = new byte[bufferSize];
        var bufferB = new byte[bufferSize];

        int readA, readB;
        while ((readA = fsA.Read(bufferA, 0, bufferA.Length)) > 0)
        {
            readB = fsB.Read(bufferB, 0, bufferB.Length);
            if (readA != readB)               // should never happen because sizes match
                return false;

            // Compare the two buffers byte‑by‑byte.
            for (int i = 0; i < readA; i++)
            {
                if (bufferA[i] != bufferB[i])
                    return false;
            }
        }

        // All chunks matched.
        return true;
    }
}
