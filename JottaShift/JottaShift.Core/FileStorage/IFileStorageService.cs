namespace JottaShift.Core.FileStorage;

/// <remarks>
/// Return-types are currently being converted to Result
/// </remarks>
public interface IFileStorageService
{
    // Done
    Result DeleteDirectoryContent(string directoryFullPath);

    Result DeleteFile(string fileFullPath);

    Result IsValidFileName(string fileNameWithExtension);

    Result<string> GetFileName(string fileFullPath);
    
    Result<DateTime> GetImageDate(string fileFullPath);

    Result<string> GetImageResolution(string fileFullPath);

    Result<string?> SearchFileByExactName(string folderPath, string fileName, bool searchRecursively = true); // TODO: Remove nullable

    Task<Result<byte[]>> GetFileBytesAsync(string fileFullPath);

    // Keep current
    IEnumerable<string> EnumerateDirectories(string directoryFullPath);

    IEnumerable<string> EnumerateFiles(string directoryFullPath);

    // TODO

    bool FilesAreBitPerfectMatch(string pathA, string pathB, int bufferSize = 1024 * 1024);

    bool DoesFileMetadataMatch(string pathA, string pathB);

    bool ValidateDirectory(DirectoryOptions options);

    Task<CopyAsyncResult> CopyAsync(string sourceFileFullPath, string targetDirectory, CancellationToken ct = default);
}
