namespace JottaShift.Core.FileExportOrchestrator;

public record FileTransferOperationResult(
    bool Success,
    string SourceFilePath,
    string TargetFilePath,
    bool MetadataValidated,
    bool SourceFileDeleted);

public enum FileTransferJobStatus
{
    Invalid,
    NotStarted,
    InProgress,
    Completed,
    Failed
}

public record FileTransferJobResult(string Key)
{
    public string Key { get; init; } = Key;
    public FileTransferJobStatus Status { get; private set; } = FileTransferJobStatus.NotStarted;
    public string? SourceDirectoryPath { get; private set; }
    public string? TargetDirectoryPath { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<FileTransferOperationResult> FileTransferOperationResults { get; init; } = [];
    
    public bool Success => Status == FileTransferJobStatus.Completed;

    private static FileTransferJobResult CreateFromStatus(string key, FileTransferJobStatus status)
    {
        return new FileTransferJobResult(key)
        {
            Status = status
        };
    }

    public static FileTransferJobResult Invalid(string jobKey, string errorMessage)
    {
        return CreateFromStatus(jobKey, FileTransferJobStatus.Invalid) with
        {
            ErrorMessage = errorMessage
        };
    }

    public static FileTransferJobResult Failed(FileTransferJob job, string errorMessage)
    {
        return CreateFromStatus(job.Key, FileTransferJobStatus.Invalid) with
        {
            ErrorMessage = errorMessage
        };
    }

    public static FileTransferJobResult Start(FileTransferJob job)
    {
        return CreateFromStatus(job.Key, FileTransferJobStatus.InProgress) with
        {
            SourceDirectoryPath =  job.SourceDirectoryPath,
            TargetDirectoryPath = job.TargetDirectoryPath
        };
    }

    public FileTransferJobResult Fail(string errorMessage)
    {
        Status = FileTransferJobStatus.Failed;
        ErrorMessage = errorMessage;

        return this;
    }

    public FileTransferJobResult Completed()
    {
        Status = FileTransferJobStatus.Completed;
        return this;
    }
}