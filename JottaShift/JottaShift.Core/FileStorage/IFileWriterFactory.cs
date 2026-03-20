namespace JottaShift.Core.FileStorage;

public interface IFileWriterFactory
{
    IFileWriter CreateFileWriter(string fileFullPath);
}
