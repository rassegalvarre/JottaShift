using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;
using System.Security.Cryptography;

namespace JottaShift.Tests;

public class FileStorageTests
{
    [Fact]
    public void ValidateFolder_ShouldBeValidated_WhenFolderExists()
    {
        var directory = AppContext.BaseDirectory;
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { directory, new MockDirectoryData() }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new FolderOptions(directory, false);

        var result = fileStorageService.ValidateFolder(options);

        Assert.True(result);
    }

    [Fact]
    public void ValidateFolder_ShouldBeValidated_WhenFolderIsCreated()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new FolderOptions(
            Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName()),
            true);

        var result = fileStorageService.ValidateFolder(options);

        Assert.True(result);
    }

    [Fact]
    public void ValidateFolder_ShouldNotBeValidated_WhenFolderDoesNotExist()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new FolderOptions(
            Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName()),
            false);

        var result = fileStorageService.ValidateFolder(options);

        Assert.False(result);
    }

    [Fact]
    public void ValidateFolder_ShouldNotBeValidated_WhenFolderCannotBeCreated()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var options = new FolderOptions(string.Empty, true);

        var result = fileStorageService.ValidateFolder(options);

        Assert.False(result);
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
    public void GetFileTimestamp_ReturnsMinValue_WhenFileNotFound()
    {
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var timestamp = fileStorageService.GetFileTimestamp(string.Empty);

        Assert.Equal(DateTime.MinValue, timestamp);
    }

    [Fact]
    public void GetFileTimestamp_ReturnsLocalCreationDate_WhenFileFound()
    {
        var directory = AppContext.BaseDirectory;
        var filePath = Path.Combine(directory, Path.GetRandomFileName());

        var creationTime = DateTime.Now;
        var fileData = new MockFileData([])
        {
            CreationTime = creationTime,
        };

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { directory, new MockDirectoryData() },
            { filePath, fileData },
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var timestamp = fileStorageService.GetFileTimestamp(filePath);

        Assert.Equal(creationTime, timestamp);
    }

    [Fact]
    public async Task CopyFile_ShouldDoNothing_WhenSourceDoesNotExist()
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

        await fileStorageService.CopyAsync(sourceFileName, destinationFileName, false);

        var copied = fileSystemMock.File.Exists(destination);

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

        await fileStorageService.CopyAsync(sourceFileName, destination, false);
        
        var sourceExists = fileSystemMock.File.Exists(sourceFileName);
        var copied = fileSystemMock.File.Exists(destinationFileName);

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

        await fileStorageService.CopyAsync(sourceFileName, destination, true);

        var sourceExists = fileSystemMock.File.Exists(sourceFileName);
        var copied = fileSystemMock.File.Exists(destinationFileName);

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

        await fileStorageService.CopyAsync(sourceFileName, destination, false);

        var sourceExists = fileSystemMock.File.Exists(sourceFileName);
        var copied = fileSystemMock.File.Exists(destinationFileName);

        Assert.True(sourceExists);
        Assert.True(copied);
    }
}
