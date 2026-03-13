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

    public bool DeleteFile(string fileFullPath)
    {
        try
        {
            _fileSystem.File.Delete(fileFullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not delete file {FilePath}. Exception: {ExceptionMessage}",
                fileFullPath,
                ex.Message);
        }

        return _fileSystem.File.Exists(fileFullPath) == false;
    }

    public bool DeleteDirectoryContent(string directoryFullPath)
    {
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

        return isEmpty;
    }

    public async Task<CopyAsyncResult> CopyAsync(string sourceFileFullPath, string targetDirectory, CancellationToken ct = default)
    {
        string fileName = Path.GetFileName(sourceFileFullPath);
        string newFileName = Path.Combine(targetDirectory, fileName);

        if (!_fileSystem.File.Exists(sourceFileFullPath))
        {
            _logger.LogWarning("Source file not found: {FilePath}", sourceFileFullPath);
            return new CopyAsyncResult(false, newFileName);
        }

        if (!ValidateDirectory(new DirectoryOptions(targetDirectory, true)))
        {
            _logger.LogWarning("Could not find or create target directory: {TargeDirectory}", targetDirectory);
            return new CopyAsyncResult(false, newFileName);
        }

        if (_fileSystem.File.Exists(newFileName))
        {
            _logger.LogInformation("Target file already exists: {TargeFile}", newFileName);
            return new CopyAsyncResult(true, newFileName);
        }

        try
        {
            
            _fileSystem.File.Copy(sourceFileFullPath, newFileName, false);
        }
        catch(Exception ex)
        {
            _logger.LogError("Exception when copying file: {ExceptionMessage}", ex.Message);
            return new CopyAsyncResult(false, newFileName);
        }

        await Task.FromResult(true);
        return new CopyAsyncResult(true, newFileName);
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

    public string GetImageResolution(string fileFullPath)
    {        
        var directories = GetMetadataDirectories(fileFullPath);

        if (TryGetTagValue(directories, "Image Width", out string width) &&
            TryGetTagValue(directories, "Image Height", out string height))
        {
            string widthStr = width.Split(' ')[0];
            string heightStr = height.Split(' ')[0];
            return $"{widthStr}x{heightStr}";
        }

        return string.Empty;
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

    public bool ValidateDirectory(DirectoryOptions options)
    {
        bool validated = false;
        if (_fileSystem.Directory.Exists(options.directoryFullPath))
        {
            validated = true;
        }
        else if (options.createIfNotExists)
        {
            try
            {
                var directory = _fileSystem.Directory.CreateDirectory(options.directoryFullPath);
                validated = directory.Exists;
                _logger.LogInformation(
                    "Created directory @{DirectoryFullPath}", options.directoryFullPath);
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    "An exception occured when creating directory @{DirectoryFullPath}. Exception: @{ExceptionMessage}",
                    options.directoryFullPath,
                    ex.Message);
                validated = false;
            }
        }

        return validated;
    }

    public bool FilesAreBitPerfectMatch(string pathA, string pathB, int bufferSize = 1024 * 1024) // 1 MiB default
    {
        // Quick size check – if lengths differ they can’t be identical.
        var infoA = _fileSystem.FileInfo.New(pathA);
        var infoB = _fileSystem.FileInfo.New(pathB);
        if (infoA.Length != infoB.Length)
            return false;

        // Open both streams for sequential reading.
        using var fsA = _fileSystem.FileStream.New(pathA, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var fsB = _fileSystem.FileStream.New(pathB, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bufferA = new byte[bufferSize];
        var bufferB = new byte[bufferSize];

        int readA, readB;
        while ((readA = fsA.Read(bufferA, 0, bufferA.Length)) > 0)
        {
            readB = fsB.Read(bufferB, 0, bufferB.Length);
            if (readA != readB)               // should never happen because sizes match
                return false;

            // Compare the two buffers byte‑by‑byte.
            for (int i = 0; i < readA; i++)
            {
                if (bufferA[i] != bufferB[i])
                    return false;
            }
        }

        // All chunks matched.
        return true;
    }   

    public bool DoesFileMetadataMatch(string originalFileFullPath, string copiedFileFullPath)
    {
        var originalFileMetadata = GetFileMetadata(originalFileFullPath);
        var copedFileMetadata = GetFileMetadata(copiedFileFullPath);
        var allKeys = new SortedSet<string>(originalFileMetadata.Keys.Concat(copedFileMetadata.Keys));

        bool hasMismatch = false;

        foreach (var key in allKeys)
        {
            originalFileMetadata.TryGetValue(key, out var valA);
            copedFileMetadata.TryGetValue(key, out var valB);

            if (valA == null)
            {
                _logger.LogError(
                    "Tile original file {FileName} is missing metadata field with key {Key}",
                    Path.GetFileName(originalFileFullPath),
                    key);
                hasMismatch = true;

            }
            else if (valB == null)
            {
                _logger.LogError(
                    "The copied file {FileName} is missing metadata field with key {Key}",
                    Path.GetFileName(copiedFileFullPath),
                    key);
                hasMismatch = true;
            }
            else if (!valA.Equals(valB))
            {
                _logger.LogError(
                    "The value of metadata field with key {Key} is mismatched between the original copied file {FileName}",
                    key,
                    Path.GetFileName(originalFileFullPath));
                hasMismatch = true;
            }
        }

        return !hasMismatch;
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
