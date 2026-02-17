using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Abstractions;

namespace JottaShift.Core.FileStorage;

public sealed class FileStorageService(
    IFileSystem _fileSystem,
    ILogger<FileStorageService> _logger) : IFileStorage
{
    public async Task CopyAsync(string sourceFileFullPath, string targetDirectory, bool deleteSource, CancellationToken ct = default)
    {
        _logger.LogInformation("Hello from FileStorageService");

        if (!_fileSystem.File.Exists(sourceFileFullPath))
        {
            return;
        }

        if (!ValidateDirectory(new DirectoryOptions(targetDirectory, true)))
        {
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

        }

        if (deleteSource)
        {
            try
            {
                _fileSystem.File.Delete(sourceFileFullPath);
            }
            catch (Exception ex)
            {
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

        var file = _fileSystem.FileInfo.New(fileFullPath);

        return file.CreationTime;
    }

    public IEnumerable<string> EnumerateFiles(string folderFullPath)
    {
        if (!_fileSystem.Directory.Exists(folderFullPath))
        {
            return [];
        }

        return _fileSystem.Directory.EnumerateFiles(folderFullPath, "*", SearchOption.AllDirectories);
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
}
