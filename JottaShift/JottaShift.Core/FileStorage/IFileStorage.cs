namespace JottaShift.Core.FileStorage;

public interface IFileStorage
{
    Task CopyAsync(string sourceFileFullPath, string targetDirectory, bool deleteSource, CancellationToken ct = default);

    bool ValidateDirectory(DirectoryOptions options);

    IEnumerable<string> EnumerateDirectories(string directoryFullPath);

    IEnumerable<string> EnumerateFiles(string directoryFullPath);

    DateTime GetFileTimestamp(string fileFullPath);

    bool FilesAreBitPerfectMatch(string pathA, string pathB, int bufferSize = 1024 * 1024);

    bool DoesFileMetadataMatch(string pathA, string pathB);
}
