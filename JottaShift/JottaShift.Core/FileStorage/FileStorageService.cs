using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using MetadataExtractor;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JottaShift.Core.FileStorage;

public sealed class FileStorageService(
    IFileSystem _fileSystem,
    ILogger<FileStorageService> _logger) : IFileStorageService
{
    public Result<string> CopyFile(string sourceFileFullPath, string targetDirectory)
    {
        var fileNameResult = GetFileName(sourceFileFullPath);
        if (!fileNameResult.Succeeded || fileNameResult.Value is null)
        {
            _logger.LogWarning("The name of the provided source file is invalid: {FilePath}",
                sourceFileFullPath);
            return Result<string>.Failure("Invalid source file path");
        }

        string newFileName = Path.Combine(targetDirectory, fileNameResult.Value);

        if (!_fileSystem.File.Exists(sourceFileFullPath))
        {
            _logger.LogWarning("Source file not found: {FilePath}", sourceFileFullPath);
            return Result<string>.Failure("Source file not found");
        }

        if (!ValidateDirectory(targetDirectory).Succeeded)
        {
            _logger.LogWarning("Could not find or create target directory: {TargeDirectory}", targetDirectory);
            return Result<string>.Failure("Target directory does not exist");
        }

        if (_fileSystem.File.Exists(newFileName))
        {
            _logger.LogInformation("Target file already exists: {TargeFile}", newFileName);
            return Result<string>.Success(newFileName);
        }

        try
        {
            _fileSystem.File.Copy(sourceFileFullPath, newFileName, false);

            if (_fileSystem.File.Exists(newFileName))
            {
                return Result<string>.Success(newFileName);
            }
            else
            {
                _logger.LogError("File-copy was executed, but target file was not created. Target file: {TargetFileName}",
                    newFileName);
                return Result<string>.Failure("File was not copied");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception when copying file to target: {TargetFileName}", newFileName);
            return Result<string>.Failure("Error when copying file");
        }
    }


    public Result DeleteDirectoryContent(string directoryFullPath)
    {
        if (!_fileSystem.Directory.Exists(directoryFullPath))
        {
            _logger.LogError("Directory does not exist and content cannot be deleted: {Directory}",
                directoryFullPath);
            return Result.Failure("Directory not found");
        }

        try
        {
            foreach (var file in _fileSystem.Directory.EnumerateFiles(directoryFullPath))
            {
                var path = Path.GetDirectoryName(file);
                _fileSystem.File.Delete(file);
            }

            foreach (var dir in _fileSystem.Directory.EnumerateDirectories(directoryFullPath))
            {
                _fileSystem.Directory.Delete(dir, recursive: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not delete directory {DirectoryPath}. Exception: {ExceptionMessage}",
                directoryFullPath,
                ex.Message);
        }

        bool isEmpty = !_fileSystem.Directory.EnumerateFileSystemEntries(directoryFullPath).Any();

        if (!isEmpty)
        {
            _logger.LogWarning("Did not succeed in deleting all the content of directory {Directory}",
                directoryFullPath);
        }

        return isEmpty ?
            Result.Success() :
            Result.Failure("Directory content was not successfully deleted");
    }

    public Result DeleteFile(string fileFullPath)
    {
        if (!Path.IsPathFullyQualified(fileFullPath))
        {
            _logger.LogError("The provided file-path is not valid: {FilePath}", fileFullPath);
            return Result.Failure("Invalid file path");
        }

        if (!_fileSystem.File.Exists(fileFullPath))
        {
            _logger.LogWarning("Attempted to delete a file that does not exist: {FilePath}",
                fileFullPath);
            return Result.Success();
        }

        try
        {
            _fileSystem.File.Delete(fileFullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured when deleting file with path {FilePath}",
                fileFullPath);
        }

        return _fileSystem.File.Exists(fileFullPath) == false ?
            Result.Success() :
            Result.Failure("File was not deleted");
    }

    public Result DoesFileMetadataMatch(string filePathA, string filePathB)
    {
        var metadataFileA = GetFileMetadata(filePathA);
        var metadataFileB = GetFileMetadata(filePathB);
        var allKeys = new SortedSet<string>(metadataFileA.Keys.Concat(metadataFileB.Keys));

        foreach (var key in allKeys)
        {
            metadataFileA.TryGetValue(key, out var valueA);
            metadataFileB.TryGetValue(key, out var valueB);

            if (valueA == null)
            {
                _logger.LogError("Metadata field {Key} is missing from file {FilePathA}",
                    filePathA, key);
                return Result.Failure("Missing metadata detected");

            }
            else if (valueB == null)
            {
                _logger.LogError("Metadata field {Key} is missing from file {FilePathB}",
                    filePathB, key);
                return Result.Failure("Missing metadata detected");
            }
            else if (!valueA.Equals(valueB))
            {
                _logger.LogError("The value of metadata field {Key} is mismatched between the provided files." +
                    "File A: {FilePathA}." +
                    "File B: {FilePathB}",
                    key, filePathA, filePathB);
                return Result.Failure("Detected mismatched metadata values");
            }
        }

        return Result.Success();
    }


    public Result FilesAreBitPerfectMatch(string pathA, string pathB, int bufferSize = 1024 * 1024) // 1 MiB default
    {
        // Quick size check – if lengths differ they can’t be identical.
        var infoA = _fileSystem.FileInfo.New(pathA);
        var infoB = _fileSystem.FileInfo.New(pathB);
        if (infoA.Length != infoB.Length)
        {
            _logger.LogError("Files A and B have different sizes. {FileA} compared to {FileB}",
                infoA.Length,
                infoB.Length);
            return Result.Failure("Files have different sizes");
        }

        // Open both streams for sequential reading.
        using var fsA = _fileSystem.FileStream.New(pathA, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var fsB = _fileSystem.FileStream.New(pathB, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bufferA = new byte[bufferSize];
        var bufferB = new byte[bufferSize];

        int readA, readB;
        while ((readA = fsA.Read(bufferA, 0, bufferA.Length)) > 0)
        {
            readB = fsB.Read(bufferB, 0, bufferB.Length);
            // should never happen because sizes match
            if (readA != readB)
            {
                return Result.Failure("Unequal chunck index");
            }

            // Compare the two buffers byte‑by‑byte.
            for (int i = 0; i < readA; i++)
            {
                if (bufferA[i] != bufferB[i])
                {
                    return Result.Failure("File inequality detected");
                }
            }
        }

        // All chunks matched.
        return Result.Success();
    }

    public Result IsValidFileName(string fileNameWithExtension)
    {
        if (string.IsNullOrWhiteSpace(fileNameWithExtension))
        {
            return Result.Failure("Filename is empty");
        }

        var invalidCharacters = Path.GetInvalidFileNameChars();
        if (fileNameWithExtension.Any(c => invalidCharacters.Contains(c)))
        {
            return Result.Failure("Filename contains invalid characters");
        }

        var split = fileNameWithExtension.Split('.');
        if (split.Length == 1 || string.IsNullOrEmpty(split.Last()))
        {
            return Result.Failure("Filename does not contain an extension");
        }

        return Result.Success();
    }

    public Result ValidateDirectory(string directoryFullPath)
    {
        if (!Path.IsPathFullyQualified(directoryFullPath))
        {
            _logger.LogError("The provided directory path is not valid: {DirectoryPath}", directoryFullPath);
            return Result.Failure("Invalid directory path");
        }

        if (_fileSystem.Directory.Exists(directoryFullPath))
        {
            return Result.Success();
        }
       
        try
        {
            var directory = _fileSystem.Directory.CreateDirectory(directoryFullPath);

            if (_fileSystem.Directory.Exists(directory.FullName))
            {
                _logger.LogInformation(
                    "Created directory {DirectoryFullPath}", directory.FullName);
                return Result.Success();
            }
            else
            {
                _logger.LogError("Directory was reported as created, but not found on disk. Path: {DirectoryPath}",
                    directory.FullName);
                return Result.Failure("Directory was not created");
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,
                "An exception occured when creating directory {DirectoryFullPath}",
                directoryFullPath);
            return Result.Failure("An exception occured when creating the directory");
        }        
    }

    public Result<string> GetFileName(string fileFullPath)
    {
        if (!Path.IsPathRooted(fileFullPath) || !Path.IsPathFullyQualified(fileFullPath))
        {
            return Result<string>.Failure("File path must be absolute and fully qualified");
        }

        var fileName = Path.GetFileName(fileFullPath);
        if (!IsValidFileName(fileName).Succeeded)
        {
            return Result<string>.Failure("File name contains invalid characters");
        }

        return Result<string>.Success(fileName);
    }

    public async Task<Result<byte[]>> GetFileBytesAsync(string fileFullPath)
    {
        if (!_fileSystem.File.Exists(fileFullPath))
        {
            _logger.LogWarning("File not found: {FilePath}", fileFullPath);
            return Result<byte[]>.Failure("File not found");
        }
        try
        {
            byte[] content = await _fileSystem.File.ReadAllBytesAsync(fileFullPath);
            if (content.Length == 0)
            {
                _logger.LogWarning("File is empty: {FilePath}", fileFullPath);
                return Result<byte[]>.Failure("File is empty");
            }

            return Result<byte[]>.Success(content);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not read file {FilePath}. Exception: {ExceptionMessage}",
                fileFullPath,
                ex.Message);
            return Result<byte[]>.Failure("Could not read file");
        }
    }

    public Result<string?> SearchFileByExactName(string folderPath, string fileName, bool searchRecursively = true)
    {
        var pattern = fileName;

        var option = searchRecursively
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        foreach (var path in _fileSystem.Directory.EnumerateFiles(folderPath, pattern, option))
        {
            return Result<string?>.Success(path);
        }

        return Result<string?>.Failure("File not found");
    }    

    public Result<DateTime> GetImageDate(string fileFullPath)
    {
        if (!_fileSystem.File.Exists(fileFullPath))
        {
            _logger.LogError("The provided file was not found. Path: {FilePath}", fileFullPath);
            return Result<DateTime>.Failure("File not found");
        }

        var directories = GetMetadataDirectories(fileFullPath);
        string[] potentialTagNames = [
            "Date/Time Original",
            "Date/Time",
            "DateTime",
            "Date/Time Digitized"
        ];

        if (TryGetTagValue(directories, potentialTagNames, out string tagValue))
        {
            if (DateTime.TryParseExact(
                tagValue,
                "yyyy:MM:dd HH:mm:ss",
                null,
                System.Globalization.DateTimeStyles.None,
                out var imageDate))
            {
                return Result<DateTime>.Success(imageDate);
            }

            // Fall back to general parsing
            if (DateTime.TryParse(tagValue, out imageDate))
            {
                return Result<DateTime>.Success(imageDate);
            }
        }

        if (TryGetDateFromFilename(Path.GetFileName(fileFullPath), out DateTime dateFromFilename))
        {
            return Result<DateTime>.Success(dateFromFilename);
        }

        // Fall back to LastWriteTime
        try
        {
            var lastWriteTime = _fileSystem.File.GetLastWriteTime(fileFullPath);
            var dateDiff = DateTime.UtcNow - lastWriteTime.ToUniversalTime();

            // Ensure that LastWriteTime is neither DateTime.Now or MinDate/default
            if (dateDiff.TotalMinutes < 1 || lastWriteTime.Year == DateTime.MinValue.Year)
            {
                return Result<DateTime>.Failure("The LastWriteTime is no valid indicator for the files origin date");
            }

            return Result<DateTime>.Success(lastWriteTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An exception occured when trying to get the last write time for file with path: {FilePath}",
                fileFullPath);
            return Result<DateTime>.Failure("Could not extract a valid date from the provided image-file");
        }
    }

    public Result<string> GetImageResolution(string fileFullPath)
    {
        if (!_fileSystem.File.Exists(fileFullPath))
        {
            _logger.LogError("Could not find file with path {FilePath}", fileFullPath);
            return Result<string>.Failure("File not found");
        }

        var directories = GetMetadataDirectories(fileFullPath);

        if (TryGetTagValue(directories, "Image Width", out string widthTagValue) &&
            TryGetTagValue(directories, "Image Height", out string heightTagValue))
        {
            // Dimension-value in metadata-tag is returned as "<value> pixels", e.g. "1080 pixels"
            // Strip away the string-portion and return conventional resolution string, e.g. "1920x1080"
            if (int.TryParse(widthTagValue.Split(' ')[0], out int width) && 
                int.TryParse(heightTagValue.Split(' ')[0], out int height))
            {
                return Result<string>.Success($"{width}x{height}");
            }
            else
            {
                return Result<string>.Failure("Extracted width and height is not valid");
            }

        }

        return Result<string>.Failure("Could not find and extract metadata tag containing image resolution");
    }

    private bool TryGetDateFromFilename(string fileName, out DateTime date)
    {
        try
        {
            // Pattern matches filenames like: img_20250215_*, photo_2025-02-15_*, etc.
            var patterns = new[]
            {
                @"(?:img|photo|picture|photo)_?(\d{8})",      // img_20250215 or img20250215
                @"(\d{4})-(\d{2})-(\d{2})",                    // 2025-02-15
                @"(\d{4})(\d{2})(\d{2})"                       // 20250215
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (match.Groups.Count == 2 && match.Groups[1].Value.Length == 8)
                    {
                        // 8-digit format: YYYYMMDD
                        var dateStr = match.Groups[1].Value;
                        if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out date))
                        {
                            return true;
                        }
                    }
                    else if (match.Groups.Count == 4)
                    {
                        // YYYY-MM-DD or YYYYMMDD format with groups
                        var year = match.Groups[1].Value;
                        var month = match.Groups[2].Value;
                        var day = match.Groups[3].Value;
                        if (DateTime.TryParse($"{year}-{month}-{day}", out date))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Could not extract date from filename: {ExceptionMessage}", ex.Message);
        }

        date = default;
        return false;
    }

    public IEnumerable<string> EnumerateDirectories(string directoryFullPath)
    {
        if (!_fileSystem.Directory.Exists(directoryFullPath))
        {
            return [];
        }

        return _fileSystem.Directory.EnumerateDirectories(directoryFullPath);
    }


    public IEnumerable<string> EnumerateFiles(string directoryFullPath)
    {
        if (!_fileSystem.Directory.Exists(directoryFullPath))
        {
            return [];
        }

        return _fileSystem.Directory.EnumerateFiles(directoryFullPath, "*", SearchOption.AllDirectories);
    }    

    private SortedDictionary<string, string> GetFileMetadata(string fileFullPath)
    {
        var directories = GetMetadataDirectories(fileFullPath);
        var map = new SortedDictionary<string, string>(); // sorted for deterministic output

        foreach (var dir in directories)
        {
            foreach (var tag in dir.Tags)
            {
                // Key format: "DirectoryName:TagName"
                string key = $"{dir.Name}:{tag.Name}";
                map[key] = tag.Description ?? string.Empty;
            }
        }

        return map;
    }

    private bool TryGetTagValue(
        IEnumerable<MetadataExtractor.Directory> directories,
        string tagName,
        out string tagValue)
    {
        bool hasValue = false;
        tagValue = string.Empty;

        var tag = directories
                  .SelectMany(d => d.Tags)
                  .FirstOrDefault(t => t.Name == tagName &&
                      !string.IsNullOrEmpty(t.Description));

        if (tag != null && !string.IsNullOrEmpty(tag.Description))
        {
            tagValue = tag.Description;
            hasValue = true;
        }

        return hasValue;
    }

    private bool TryGetTagValue(IEnumerable<MetadataExtractor.Directory> directories,
        string[] tagNames,
        out string tagValue)
    {
        foreach (var dir in directories)
        {
            foreach (var tag in dir.Tags)
            {
                if (tagNames.Contains(tag.Name) && !string.IsNullOrEmpty(tag.Description))
                {
                    tagValue = tag.Description;
                    return true;
                }
            }           
        }

        tagValue = string.Empty;
        return false;
    }

    private IReadOnlyList<MetadataExtractor.Directory> GetMetadataDirectories(string fileFullPath)
    {
        var defaultDirectories = Enumerable.Empty<MetadataExtractor.Directory>().ToImmutableList();

        if (!_fileSystem.File.Exists(fileFullPath))
            return defaultDirectories;

        try
        {
            using var fileStream = _fileSystem.File.OpenRead(fileFullPath);
            var directories = ImageMetadataReader.ReadMetadata(fileStream, fileFullPath);
            
            return directories;
        }
        catch(Exception ex)
        {
            _logger.LogWarning("Could not extract metadata directory: {ExceptionMessage}", ex.Message);
        }

        return defaultDirectories;
    }
}
