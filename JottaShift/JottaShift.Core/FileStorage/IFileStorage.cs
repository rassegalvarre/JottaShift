using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.FileStorage;

public interface IFileStorage
{
    Task CopyAsync(string sourcePath, string targetPath, CancellationToken ct = default);

    bool ValidateFolder(FolderOptions options);
}
