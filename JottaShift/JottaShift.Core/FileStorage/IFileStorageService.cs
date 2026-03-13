namespace JottaShift.Core.FileStorage;

public interface IFileStorageService
{
    Result<string> CopyFile(string sourceFileFullPath, string targetDirectory);

    Result DeleteDirectoryContent(string directoryFullPath);

    Result DeleteFile(string fileFullPath);
    
    Result DoesFileMetadataMatch(string filePathA, string filePathB);

    Result FilesAreBitPerfectMatch(string pathA, string pathB, int bufferSize = 1024 * 1024);

    Result IsValidFileName(string fileNameWithExtension);

    Result ValidateDirectory(string directoryFullPath);

    Result<string> GetFileName(string fileFullPath);
    
    Result<DateTime> GetImageDate(string fileFullPath);

    Result<string> GetImageResolution(string fileFullPath);

    Result<string> SearchFileByExactName(string folderPath, string fileName, bool searchRecursively = true);

    Task<Result<byte[]>> GetFileBytesAsync(string fileFullPath);

    IEnumerable<string> EnumerateDirectories(string directoryFullPath);

    IEnumerable<string> EnumerateFiles(string directoryFullPath);
}
