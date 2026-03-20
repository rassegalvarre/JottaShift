using System.IO.Abstractions;

namespace JottaShift.Tests.FileStorage;

public static class FileSystemAssert
{
    public static void AssertFileExists(this IFileSystem fileSystem, string? filePath)
    {
        Assert.True(
            fileSystem.File.Exists(filePath), 
            "Expected file to exist, but it was not found");
    }

    public static void AssertFileDoesNotExist(this IFileSystem fileSystem, string? filePath)
    {
        Assert.False(
            fileSystem.File.Exists(filePath),
            "Expected file to not exist, but it was found");
    }

    public static void AssertDirectoryExists(this IFileSystem fileSystem, string? directoryPath)
    {
        Assert.True(
            fileSystem.Directory.Exists(directoryPath),
            "Expected directory to exist, but it was not found");
    }

    public static void AssertDirectoryDoesNotExist(this IFileSystem fileSystem, string? directoryPath)
    {
        Assert.False(
            fileSystem.Directory.Exists(directoryPath),
            "Expected directory to not exist, but it was found");
    }

    public static void AssertDirectoryIsEmpty(this IFileSystem fileSystem, string directoryPath)
    {
        Assert.Empty(
            fileSystem.Directory.EnumerateFileSystemEntries(directoryPath));
    }
    public static void AssertDirectoryIsNotEmpty(this IFileSystem fileSystem, string directoryPath)
    {
        Assert.NotEmpty(
            fileSystem.Directory.EnumerateFileSystemEntries(directoryPath));
    }
}
