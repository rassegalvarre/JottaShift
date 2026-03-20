using JottaShift.Core.FileStorage;

namespace JottaShift.Tests.FileStorage;

public class FileWriterFactoryMock : IFileWriterFactory
{
    public IFileWriter CreateFileWriter(string fileFullPath)
    {
        return new FileWriter(fileFullPath);
    }
}
