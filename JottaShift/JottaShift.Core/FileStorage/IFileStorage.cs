using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.FileStorage;

public interface IFileStorage
{
    Task CopyAsync(string sourceFileFullPath, string targetDirectory, bool deleteSource, CancellationToken ct = default);

    bool ValidateDirectory(DirectoryOptions options);

    IEnumerable<string> EnumerateFiles(string folderFullPath);

    DateTime GetFileTimestamp(string fileFullPath);
}
