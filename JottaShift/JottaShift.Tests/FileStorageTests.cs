using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace JottaShift.Tests;

public class FileStorageTests
{
    private static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string Duck = Path.Combine(TestDataPath, "duck.jpg");
    private static readonly string DuckCopy = Path.Combine(TestDataPath, "duck_copy.jpg");
    private static readonly string Waterfall = Path.Combine(TestDataPath, "waterfall.jpg");

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
    public void GetFileTimestampFromLastWriteTime_ReturnsMinValue_WhenFileNotFound()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var timestamp = fileStorageService.GetFileTimestampFromLastWriteTimeFromLastWriteTime(string.Empty);

        Assert.Equal(DateTime.MinValue, timestamp);
    }

    [Fact]
    public void GetFileTimestampFromLastWriteTime_ReturnsLocalCreationDate_WhenFileFound()
    {
        var directory = AppContext.BaseDirectory;
        var filePath = Path.Combine(directory, Path.GetRandomFileName());

        var creationDate = new DateTime(2010, 6, 15);
        var fileData = new MockFileData([])
        {
            CreationTime = creationDate,
        };

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { directory, new MockDirectoryData() },
            { filePath, fileData },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var timestamp = fileStorageService.GetFileTimestampFromLastWriteTimeFromLastWriteTime(filePath);

        Assert.Equal(creationDate, timestamp);
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

        bool filesAreBitPerfectMatch = fileStorageService.FilesAreBitPerfectMatch(Duck, DuckCopy);

        Assert.True(filesAreBitPerfectMatch);
    }

    [Fact]
    public void DoesFileMetadataMatch_DoesNotMatch_WhenDifferentImages()
    {
        var fileStorageService = new FileStorageService(
            new FileSystem(),
            new Mock<ILogger<FileStorageService>>().Object);

        bool metadataMatches = fileStorageService.DoesFileMetadataMatch(Duck, Waterfall);

        Assert.False(metadataMatches);
    }

    [Fact]
    public void DoesFileMetadataMatch_DoesMatch_WhenCopyOfSameImage()
    {
        var fileStorageService = new FileStorageService(
            new FileSystem(),
            new Mock<ILogger<FileStorageService>>().Object);

        bool metadataMatches = fileStorageService.DoesFileMetadataMatch(Duck, DuckCopy);

        Assert.True(metadataMatches);
    }
}
