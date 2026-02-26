using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace JottaShift.Tests;

public class FileStorageTests
{
    [Fact]
    public void ValidateDirectory_ShouldBeValidated_WhenFolderExists()
    {
        var directory = AppContext.BaseDirectory;
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { directory, new MockDirectoryData() }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new DirectoryOptions(directory, false);

        var result = fileStorageService.ValidateDirectory(options);

        Assert.True(result);
    }

    [Fact]
    public void ValidateDirectory_ShouldBeValidated_WhenFolderIsCreated()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new DirectoryOptions(
            Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName()),
            true);

        var result = fileStorageService.ValidateDirectory(options);

        Assert.True(result);
    }

    [Fact]
    public void ValidateDirectory_ShouldNotBeValidated_WhenFolderDoesNotExist()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new DirectoryOptions(
            Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName()),
            false);

        var result = fileStorageService.ValidateDirectory(options);

        Assert.False(result);
    }

    [Fact]
    public void ValidateDirectory_ShouldNotBeValidated_WhenFolderCannotBeCreated()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new DirectoryOptions(string.Empty, true);

        var result = fileStorageService.ValidateDirectory(options);

        Assert.False(result);
    }

    [Fact]
    public void EnumerateDirectories_ShouldEnumerate_EmptyCollectionOnInvalidPath()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var collection = fileStorageService.EnumerateFiles(string.Empty);

        Assert.Empty(collection);
    }

    [Fact]
    public void EnumerateDirectories_ShouldEnumerate_FlatStructure()
    {
        var baseDirectory = AppContext.BaseDirectory;

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { baseDirectory, new MockDirectoryData() },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockDirectoryData() },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockDirectoryData() }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var collection = fileStorageService.EnumerateDirectories(baseDirectory);

        Assert.Equal(2, collection.Count());
    }

    [Fact]
    public void EnumerateDirectories_ShouldEnumerateTopLevel_WhenMultiLayered()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var firstSubLevelDirectoryOne = Path.Combine(baseDirectory, Path.GetRandomFileName());
        var firstSubLevelDirectoryTwo = Path.Combine(baseDirectory, Path.GetRandomFileName());
        var secondSubLevelDirectory = Path.Combine(firstSubLevelDirectoryOne, Path.GetRandomFileName());
        var thirdSubLevelDirectory = Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName());

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { baseDirectory, new MockDirectoryData() },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { firstSubLevelDirectoryOne, new MockDirectoryData() },
            { Path.Combine(firstSubLevelDirectoryOne, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(firstSubLevelDirectoryOne, Path.GetRandomFileName()), new MockFileData([]) },
            { firstSubLevelDirectoryTwo, new MockDirectoryData() },

            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockDirectoryData() },
            { Path.Combine(thirdSubLevelDirectory, Path.GetRandomFileName()), new MockDirectoryData() }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var collection = fileStorageService.EnumerateDirectories(baseDirectory);

        Assert.Equal(2, collection.Count());
    }

    [Fact]
    public void EnumerateFiles_ShouldEnumerate_EmptyCollectionOnInvalidPath()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var collection = fileStorageService.EnumerateFiles(string.Empty);

        Assert.Empty(collection);
    }

    [Fact]
    public void EnumerateFiles_ShouldEnumerate_FlatStructure()
    {
        var directory = AppContext.BaseDirectory;
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { directory, new MockDirectoryData() },
            { Path.Combine(directory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(directory, Path.GetRandomFileName()), new MockFileData([]) }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var collection = fileStorageService.EnumerateFiles(directory);

        Assert.Equal(2, collection.Count());
    }

    [Fact]
    public void EnumerateFiles_ShouldEnumerate_Recursive()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var firstSubLevelDirectory = Path.Combine(baseDirectory, Path.GetRandomFileName());
        var secondSubLevelDirectory = Path.Combine(firstSubLevelDirectory, Path.GetRandomFileName());
        var thirdSubLevelDirectory = Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName());

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { baseDirectory, new MockDirectoryData() },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { secondSubLevelDirectory, new MockDirectoryData() },
            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { thirdSubLevelDirectory, new MockDirectoryData() },
            { Path.Combine(thirdSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(thirdSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },

            { Path.Combine(baseDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(secondSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) },
            { Path.Combine(thirdSubLevelDirectory, Path.GetRandomFileName()), new MockFileData([]) }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var collection = fileStorageService.EnumerateFiles(baseDirectory);

        Assert.Equal(10, collection.Count());
    }

    [Fact]
    public void GetImageDate_ReturnsMinValue_WhenFileNotFound()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var timestamp = fileStorageService.GetImageDate(string.Empty);

        Assert.Equal(DateTime.MinValue, timestamp);
    }

    [Fact]
    public void GetImageDate_ReturnsDateTakenExiffTag_WhenFileFound()
    {
        var fileSystem = new FileSystem();

        var fileStorageService = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);

        var timestamp = fileStorageService.GetImageDate(TestData.Duck);

        Assert.Equal(2025, timestamp.Year);
        Assert.Equal(5, timestamp.Month);
        Assert.Equal(17, timestamp.Day);
        Assert.Equal(13, timestamp.Hour);
        Assert.Equal(42, timestamp.Minute);
    }

    [Theory]
    [MemberData(nameof(GetImageFilenameTestData))]
    public void GetImageDate_ReturnsDateFromFileName_WhenNoMetadataFound(string filename, int expectedYear, int expectedMonth, int expectedDay)
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filename, new MockFileData([]) }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var timestamp = fileStorageService.GetImageDate(filename);

        Assert.Equal(expectedYear, timestamp.Year);
        Assert.Equal(expectedMonth, timestamp.Month);
        Assert.Equal(expectedDay, timestamp.Day);
    }

    public static IEnumerable<object[]> GetImageFilenameTestData()
    {
        return new List<object[]>
        {
            // Underscore prefix with 8-digit format (img_YYYYMMDD)
            new object[] { "img_20250517.jpg", 2025, 5, 17 },
            new object[] { "img_20241225.png", 2024, 12, 25 },
            new object[] { "photo_20230815.jpg", 2023, 8, 15 },
            new object[] { "picture_20220101.jpg", 2022, 1, 1 },

            // Hyphenated format (YYYY-MM-DD)
            new object[] { "vacation_2025-05-17.jpg", 2025, 5, 17 },
            new object[] { "photo_2024-12-25.png", 2024, 12, 25 },
            new object[] { "2025-05-17_family_photo.jpg", 2025, 5, 17 },
            new object[] { "2024-12-25_christmas.jpg", 2024, 12, 25 },

            // Compact 8-digit format (YYYYMMDD)
            new object[] { "20250517_beach.jpg", 2025, 5, 17 },
            new object[] { "20241225_holiday.png", 2024, 12, 25 },
            new object[] { "20230815_vacation.jpg", 2023, 8, 15 },

            // Camera default filenames with dates
            new object[] { "IMG_20250517_134210.jpg", 2025, 5, 17 },
            new object[] { "PHOTO_20241225_090000.jpg", 2024, 12, 25 },
            new object[] { "DSC_20230815_143022.jpg", 2023, 8, 15 },

            // Complex filenames with dates in the middle
            new object[] { "IMG_20250517_landscape_001.jpg", 2025, 5, 17 },
            new object[] { "photo_2025-05-17_sunset.jpg", 2025, 5, 17 },
            new object[] { "vacation_20250517_moment.jpg", 2025, 5, 17 },

            // Alternative formats
            new object[] { "pic-2025-05-17-001.jpg", 2025, 5, 17 },
        };
    }

    [Fact]
    public async Task CopyFile_ShouldFail_WhenSourceDoesNotExist()
    {
        var source = AppContext.BaseDirectory;
        var destination = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName());

        var fileName = Path.GetRandomFileName();
        var sourceFileName = Path.Combine(source, fileName);
        var destinationFileName = Path.Combine(destination, fileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { source, new MockDirectoryData() }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = await fileStorageService.CopyAsync(sourceFileName, destinationFileName, false);

        var copied = fileSystemMock.File.Exists(destination);

        Assert.False(result.Success);        
        Assert.False(copied);        
    }  

    [Fact]
    public async Task CopyFile_ShouldCopy_WhenValidSourceAndDestination()
    {
        var source = AppContext.BaseDirectory;
        var destination = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName());

        var fileName = Path.GetRandomFileName();
        var sourceFileName = Path.Combine(source, fileName);
        var destinationFileName = Path.Combine(destination, fileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { source, new MockDirectoryData() },
            { sourceFileName, new MockFileData([]) },
            { destination, new MockDirectoryData() },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = await fileStorageService.CopyAsync(sourceFileName, destination, false);
        
        var sourceExists = fileSystemMock.File.Exists(sourceFileName);
        var copied = fileSystemMock.File.Exists(destinationFileName);

        Assert.True(result.Success);
        Assert.True(sourceExists);
        Assert.True(copied);
    }

    [Fact]
    public async Task CopyFile_ShouldCopyAndDeleteSource_WhenValidSourceAndDestination()
    {
        var source = AppContext.BaseDirectory;
        var destination = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName());

        var fileName = Path.GetRandomFileName();
        var sourceFileName = Path.Combine(source, fileName);
        var destinationFileName = Path.Combine(destination, fileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { source, new MockDirectoryData() },
            { sourceFileName, new MockFileData([]) },
            { destination, new MockDirectoryData() },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = await fileStorageService.CopyAsync(sourceFileName, destination, true);

        var sourceExists = fileSystemMock.File.Exists(sourceFileName);
        var copied = fileSystemMock.File.Exists(destinationFileName);

        Assert.True(result.Success);
        Assert.False(sourceExists);
        Assert.True(copied);
    }

    [Fact]
    public async Task CopyFile_ShouldCreateTargetDirectory_WhenNotExists()
    {
        var source = AppContext.BaseDirectory;
        var destination = Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName());

        var fileName = Path.GetRandomFileName();
        var sourceFileName = Path.Combine(source, fileName);
        var destinationFileName = Path.Combine(destination, fileName);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { source, new MockDirectoryData() },
            { sourceFileName, new MockFileData([]) },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var result = await fileStorageService.CopyAsync(sourceFileName, destination, false);

        var sourceExists = fileSystemMock.File.Exists(sourceFileName);
        var copied = fileSystemMock.File.Exists(destinationFileName);

        Assert.True(result.Success);
        Assert.True(sourceExists);
        Assert.True(copied);
    }

    [Fact]
    public void FilesAreBitPerfectMatch_DoesNotMatch_WhenDifferentContentLength()
    {
        string fileA = Path.GetRandomFileName();
        var fileAContent = Enumerable.Range(0, 8)
            .Select(x => (byte)x)
            .ToArray();

        string fileB = Path.GetRandomFileName();
        var fileBContent = Enumerable.Range(0, 16)
          .Select(x => (byte)x)
          .ToArray();

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { fileA, new MockFileData(fileAContent) },
            { fileB, new MockFileData(fileBContent) },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        bool filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(fileA, fileB);

        Assert.False(filesAreBitPerfectMatch);
    }

    [Fact]
    public void FilesAreBitPerfectMatch_DoesNotMatch_WhenDifferentContent()
    {
        string fileA = Path.GetRandomFileName();
        var fileAContent = Enumerable.Range(0, 8)
            .Select(x => (byte)x)
            .ToArray();

        string fileB = Path.GetRandomFileName();
        var fileBContent = Enumerable.Range(0, 8)
          .Select(x => (byte)(x * 2))
          .ToArray();

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { fileA, new MockFileData(fileAContent) },
            { fileB, new MockFileData(fileBContent) },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        bool filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(fileA, fileB);

        Assert.False(filesAreBitPerfectMatch);
    }

    [Fact]
    public void FilesAreBitPerfectMatch_DoesMatch_WhenEqualContent()
    {
        string fileA = Path.GetRandomFileName();
        var fileAContent = Enumerable.Range(0, 64)
            .Select(x => (byte)(x * 2))
            .ToArray();

        string fileB = Path.GetRandomFileName();
        var fileBContent = Enumerable.Range(0, 64)
          .Select(x => (byte)(x * 2))
          .ToArray();

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { fileA, new MockFileData(fileAContent) },
            { fileB, new MockFileData(fileBContent) },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        bool filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(fileA, fileB);

        Assert.True(filesAreBitPerfectMatch);
    }

    [Fact]
    public void FilesAreBitPerfectMatch_DoesMatch_WhenCopyOfImage()
    {
        var fileStorageService = new FileStorageService(
            new FileSystem(),
            new Mock<ILogger<FileStorageService>>().Object);

        bool filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(TestData.Duck, TestData.DuckCopy);

        Assert.True(filesAreBitPerfectMatch);
    }

    [Fact]
    public void DoesFileMetadataMatch_DoesNotMatch_WhenDifferentImages()
    {
        var fileStorageService = new FileStorageService(
            new FileSystem(),
            new Mock<ILogger<FileStorageService>>().Object);

        bool metadataMatches = fileStorageService.DoesFileMetadataMatch(TestData.Duck, TestData.Waterfall);

        Assert.False(metadataMatches);
    }

    [Fact]
    public void DoesFileMetadataMatch_DoesMatch_WhenCopyOfSameImage()
    {
        var fileStorageService = new FileStorageService(
            new FileSystem(),
            new Mock<ILogger<FileStorageService>>().Object);

        bool metadataMatches = fileStorageService.DoesFileMetadataMatch(TestData.Duck, TestData.DuckCopy);

        Assert.True(metadataMatches);
    }
}
