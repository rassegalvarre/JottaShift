using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.FileStorage;

public sealed class FileStorageService(ILogger<FileStorageService> _logger) : IFileStorage
{
    public async Task CopyAsync(string sourcePath, string targetPath, bool deleteSource, CancellationToken ct = default)
    {
        _logger.LogInformation("Hello from FileStorageService");

        if (File.Exists(sourcePath))
        {
            return;
        }

        if (File.Exists(targetPath))
        {
            return;
        }

        try
        {
            File.Copy(sourcePath, targetPath, false);
        }
        catch(Exception ex)
        {

        }

        if (deleteSource)
        {
            try
            {
                File.Delete(sourcePath);
            }
            catch (Exception ex)
            {
            }
        }

        await Task.FromResult(true);
    }

    public DateTime GetFileTimestamp(string fileFullPath)
    {
        if (!File.Exists(fileFullPath))
        {
            return default;
        }

        var file = new FileInfo(fileFullPath);

        return file.CreationTime;
    }

    public IEnumerable<string> EnumerateFiles(string folderFullPath)
    {
        if (!Directory.Exists(folderFullPath))
        {
            return [];
        }

        return Directory.EnumerateFiles(folderFullPath, "*", SearchOption.AllDirectories);
    }

    public bool ValidateFolder(FolderOptions options)
    {
        bool validated = false;
        if (Directory.Exists(options.folderFullPath))
        {
            validated = true;
        }
        else if (options.createIfNotExists)
        {
            try
            {
                var directory = Directory.CreateDirectory(options.folderFullPath);
                validated = directory.Exists;
                _logger.LogInformation(
                    "Created folder @{FolerFullPath}", options.folderFullPath);
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    "An exception occured when creating folder @{FolderFullPath}. Exception: @{ExceptionMessage}",
                    options.folderFullPath,
                    ex.Message);
                validated = false;
            }
        }

        return validated;
    }
}
