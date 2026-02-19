using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using MetadataExtractor;
using System.Collections.Immutable;

namespace JottaShift.Core.FileStorage;

public sealed class FileStorageService(
    IFileSystem _fileSystem,
    ILogger<FileStorageService> _logger) : IFileStorage
{
    public async Task<CopyAsyncResult> CopyAsync(string sourceFileFullPath, string targetDirectory, bool deleteSource, CancellationToken ct = default)
    {
        string fileName = Path.GetFileName(sourceFileFullPath);
        string newFileName = Path.Combine(targetDirectory, fileName);

        if (!_fileSystem.File.Exists(sourceFileFullPath))
        {
            _logger.LogWarning("Source file not found: {FilePath}", sourceFileFullPath);
            return new CopyAsyncResult(false, newFileName);
        }

        if (!ValidateDirectory(new DirectoryOptions(targetDirectory, true)))
        {
            _logger.LogWarning("Could not find or create target directory: {TargeDirectory}", targetDirectory);
            return new CopyAsyncResult(false, newFileName);
        }

        try
        {
            
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
                return new CopyAsyncResult(false, newFileName);
            }
        }

        await Task.FromResult(true);
        return new CopyAsyncResult(true, newFileName);
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

    public bool DoesFileMetadataMatch(string originalFileFullPath, string copiedFileFullPath)
    {
        var originalFileMetadata = GetFileMetadata(originalFileFullPath);
        var copedFileMetadata = GetFileMetadata(copiedFileFullPath);
        var allKeys = new SortedSet<string>(originalFileMetadata.Keys.Concat(copedFileMetadata.Keys));

        bool hasMismatch = false;

        foreach (var key in allKeys)
        {
            originalFileMetadata.TryGetValue(key, out var valA);
            copedFileMetadata.TryGetValue(key, out var valB);

            if (valA == null)
            {
                _logger.LogError(
                    "Tile original file {FileName} is missing metadata field with key {Key}",
                    Path.GetFileName(originalFileFullPath),
                    key);
                hasMismatch = true;

            }
            else if (valB == null)
            {
                _logger.LogError(
                    "The copied file {FileName} is missing metadata field with key {Key}",
                    Path.GetFileName(copiedFileFullPath),
                    key);
                hasMismatch = true;
            }
            else if (!valA.Equals(valB))
            {
                _logger.LogError(
                    "The value of metadata field with key {Key} is mismatched between the original copied file {FileName}",
                    key,
                    Path.GetFileName(originalFileFullPath));
                hasMismatch = true;
            }
        }

        return !hasMismatch;
    }

    private SortedDictionary<string, string> GetFileMetadata(string fileFullPath)
    {
        using var fileStream = _fileSystem.File.OpenRead(fileFullPath);

        var directories = ImageMetadataReader.ReadMetadata(fileStream, fileFullPath);
        var map = new SortedDictionary<string, string>(); // sorted for deterministic output

        foreach (var dir in directories)
        {
            foreach (var tag in dir.Tags)
            {
                // Key format: "DirectoryName:TagName"
                string key = $"{dir.Name}:{tag.Name}";
                map[key] = tag.Description ?? string.Empty;
            }
        }

        return map;
    }
}
