using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;

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
        var directory = @"C:\temp\myfolder";
        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { directory, new MockDirectoryData() },
            { Path.Combine(directory, "readme.txt"), new MockFileData("demo") },
            { Path.Combine(directory, "demo.txt"), new MockFileData("demo") }
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
        var baseDirectory = @"C:\temp\myfolder";
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
}
