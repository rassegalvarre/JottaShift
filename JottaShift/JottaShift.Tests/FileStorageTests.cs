using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests;

public class FileStorageTests
{
    private readonly FileStorageService fileStorageService = new(
        new Mock<ILogger<FileStorageService>>().Object);

    [Fact]
    public void ValidateFolder_ShouldBeValidated_WhenFolderExists()
    {
        var options = new FolderOptions(AppContext.BaseDirectory, false);

        var result = fileStorageService.ValidateFolder(options);

        Assert.True(result);
    }

    [Fact]
    public void ValidateFolder_ShouldBeValidated_WhenFolderIsCreated()
    {
        var options = new FolderOptions(
            Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName()),
            true);

        var result = fileStorageService.ValidateFolder(options);

        Assert.True(result);

        Directory.Delete(options.folderFullPath);
    }

    [Fact]
    public void ValidateFolder_ShouldNotBeValidated_WhenFolderDoesNotExist()
    {
        var options = new FolderOptions(
            Path.Combine(AppContext.BaseDirectory, Path.GetRandomFileName()),
            false);

        var result = fileStorageService.ValidateFolder(options);

        Assert.False(result);
    }

    [Fact]
    public void ValidateFolder_ShouldNotBeValidated_WhenFolderCannotBeCreated()
    {
        var options = new FolderOptions(string.Empty, true);

        var result = fileStorageService.ValidateFolder(options);

        Assert.False(result);
    }
}
