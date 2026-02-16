using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.FileStorage;

public sealed class FileStorageService(ILogger<FileStorageService> _logger) : IFileStorage
{
    public async Task CopyAsync(string sourcePath, string targetPath, CancellationToken ct = default)
    {
        _logger.LogInformation("Hello from FileStorageService");
        await Task.FromResult(true);
    }
}
