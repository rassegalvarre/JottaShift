namespace JottaShift.Core.FileStorage;

public interface IFileStorageService
{
    Result DeleteDirectoryContent(string directoryFullPath);

    Result DeleteFile(string fileFullPath);
    
    Result DoesFileMetadataMatch(string filePathA, string filePathB);

    /// <param name="bufferSize">1 MiB default</param>
    Result FilesAreBitPerfectMatch(string fileFullPathA, string fileFullPathB, int bufferSize = 1024 * 1024);

    Result IsValidFileName(string fileNameWithExtension);

    Result ValidateDirectory(string directoryFullPath);

    /// <returns>A <see cref="Result{T}"/> containing the full path of the copied target file</returns>
    Result<string> CopyFile(string sourceFileFullPath, string targetDirectory, string? newFileName = null);
   
    Result<string> GetFileName(string fileFullPath);    

    Result<DateTime> GetImageDate(string fileFullPath);

    /// <returns>A <see cref="Result{T}"/> containing the resolution in the format of "[width]x[height]", e.g. "1920x1080"</returns>
    Result<string> GetImageResolution(string fileFullPath);

    /// <returns>A <see cref="Result{T}"/> containing the full path of file found</returns>
    Result<string> SearchFileByExactName(string directoryFullPath, string fileName, bool searchRecursively = true);

    Task<Result<byte[]>> GetFileBytesAsync(string fileFullPath);

    IEnumerable<string> EnumerateDirectories(string directoryFullPath);

    IEnumerable<string> EnumerateFiles(string directoryFullPath);
}
