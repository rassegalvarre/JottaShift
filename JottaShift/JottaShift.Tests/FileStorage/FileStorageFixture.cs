using JottaShift.Core.FileStorage;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace JottaShift.Tests.FileStorage;

public class FileStorageFixture : IDisposable
{
    public readonly string BaseDirectory = @"C:\FileStorage";
    public string SomeValidFileName => "SomeFile.pdf";
    public string SomeValidFileFullPath => Path.Combine(BaseDirectory, SomeValidFileName);

    public FileStorageService CreateFileStorageService(IFileSystem? fileSystem = null)
    {
        fileSystem ??= new MockFileSystem();

        return new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
