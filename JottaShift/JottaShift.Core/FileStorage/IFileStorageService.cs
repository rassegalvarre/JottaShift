namespace JottaShift.Core.FileStorage;

public interface IFileStorageService
{
    Result IsValidFileName(string fileNameWithExtension);

    Result<string> GetFileName(string fileFullPath);
    
    Task<Result<byte[]>> GetFileContent(string fileFullPath);

    Result<string?> SearchFileByExactName(string folderPath, string fileName, bool searchRecursively = true);

    bool DeleteFile(string fileFullPath);
    bool DeleteDirectoryContent(string directoryFullPath);
    Task<CopyAsyncResult> CopyAsync(string sourceFileFullPath, string targetDirectory, CancellationToken ct = default);

    bool ValidateDirectory(DirectoryOptions options);

    IEnumerable<string> EnumerateDirectories(string directoryFullPath);

    IEnumerable<string> EnumerateFiles(string directoryFullPath);

    DateTime GetImageDate(string fileFullPath);

    string GetImageResolution(string fileFullPath);

    bool FilesAreBitPerfectMatch(string pathA, string pathB, int bufferSize = 1024 * 1024);

    bool DoesFileMetadataMatch(string pathA, string pathB);
}
