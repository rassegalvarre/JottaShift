namespace JottaShift.Core.FileStorage;

public class FileWriterFactory : IFileWriterFactory
{
    public IFileWriter CreateFileWriter(string fileFullPath)
    {
        return new FileWriter(fileFullPath);
    }
}
