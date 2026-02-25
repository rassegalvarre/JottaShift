using Google.Apis.PhotosLibrary.v1.Data;

namespace JottaShift.Core.FileExportOrchestrator;

public enum FileTransferOperationResultStatus
{
    NotStarted,
    InProgress,
    TransferFailed,
    InvalidFileContent,
    InvalidFileMetadata,
    Completed,
}

public record FileTransferOperationResult(string SourceFilePath)
{
    public FileTransferOperationResultStatus Status { get; private set; } = FileTransferOperationResultStatus.NotStarted;
    public string SourceFilePath { get; init; } = SourceFilePath;
    public string TargetFilePath { get; private set; } = string.Empty;
    public bool Success { get; init; }

    private static FileTransferOperationResult CreateFromStatus(string SourceFilePath, FileTransferOperationResultStatus status)
    {
        return new FileTransferOperationResult(SourceFilePath)
        {
            Status = status
        };
    }

    public static FileTransferOperationResult Prepare(string sourceFilePath)
    {
        return CreateFromStatus(sourceFilePath, FileTransferOperationResultStatus.NotStarted);
    }

    public void Start()
    {
        Status = FileTransferOperationResultStatus.InProgress;
    }

    public void TransferEnded(string targetFilePath)
    {
        TargetFilePath = targetFilePath;
    }

    public void TransferFailed(string targetFilePath)
    {
        TargetFilePath = targetFilePath;
        Status = FileTransferOperationResultStatus.TransferFailed;
    }

    public void InvalidFileContent()
    {
        Status = FileTransferOperationResultStatus.InvalidFileContent;
    }

    public void InvalidMetadata()
    {
        Status = FileTransferOperationResultStatus.InvalidFileMetadata;
    }

    public void Complete()
    {
        Status = FileTransferOperationResultStatus.Completed;
    }
}

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

    public FileTransferJobResult Fail(string errorMessage, FileTransferOperationResult operationResult)
    {
        Status = FileTransferJobStatus.Failed;
        ErrorMessage = errorMessage;
        FileTransferOperationResults.Add(operationResult);
        return this;
    }

    public void Continue(FileTransferOperationResult operationResult)
    {
        FileTransferOperationResults.Add(operationResult);
    }

    public FileTransferJobResult Completed()
    {
        Status = FileTransferJobStatus.Completed;
        return this;
    }
}