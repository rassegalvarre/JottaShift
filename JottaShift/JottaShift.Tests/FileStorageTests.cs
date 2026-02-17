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
}
