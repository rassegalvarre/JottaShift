namespace JottaShift.Core.FileExportOrchestrator;

public enum FileTransferJobStatus
{
    Invalid,
    NotStarted,
    InProgress,
    Completed,
    Failed
}

// TODO: Rename to FileExportJobResult and make abstract
// with derived classes for FileTransferJobResult and GooglePhotosUploadJobResult
public record FileTransferJobResult(string Key)
{
    public string Key { get; init; } = Key;
    public FileTransferJobStatus Status { get; private set; } = FileTransferJobStatus.NotStarted;
    public string? SourceDirectoryPath { get; private set; }
    public string? TargetDirectoryPath { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<FileTransferOperationResult> FileTransferOperationResults { get; init; } = [];

    private FileTransferOperationResult? CurrentOperation {  get; set; }

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

    public static FileTransferJobResult StartJob(FileTransferJob job)
    {
        return CreateFromStatus(job.Key, FileTransferJobStatus.InProgress) with
        {
            SourceDirectoryPath =  job.SourceDirectoryPath,
            TargetDirectoryPath = job.TargetDirectoryPath
        };
    }

    public void PrepareOperation(string sourceFilePath)
    {
        if (CurrentOperation != null)
        {
            throw new InvalidOperationException("Cannot prepare a new operation while another operation is in progress.");
        }

        var operation = FileTransferOperationResult.Prepare(sourceFilePath);
        CurrentOperation = operation;
    }

    public void StartOperation()
    {
        if (CurrentOperation == null)
        {
            throw new InvalidOperationException("Cannot start a new operation while another operation is in progress.");
        }
        
        CurrentOperation.Start();
    }

    public FileTransferJobResult FailOperation(string errorMessage)
    {
        if (CurrentOperation != null)
        {
            Status = FileTransferJobStatus.Failed;
            ErrorMessage = errorMessage;
            CurrentOperation.Fail(errorMessage);
            FileTransferOperationResults.Add(CurrentOperation);
            CurrentOperation = null;
            return this;
        }
        
        throw new InvalidOperationException("No current operation to fail");
    }

    public void CompleteOperation(string? targetFilePath)
    {
        if (CurrentOperation == null)
        {
            throw new InvalidOperationException("No current operation to complete.");
        }

        CurrentOperation.Complete(targetFilePath);
        FileTransferOperationResults.Add(CurrentOperation);
        CurrentOperation = null;        
    }

    public FileTransferJobResult CompleteJob()
    {
        if (CurrentOperation != null &&
            CurrentOperation.Status != FileTransferOperationResultStatus.Completed)
        {
            throw new InvalidOperationException("Current operation as not been marked as completed.");
        }
        
        Status = FileTransferJobStatus.Completed;
        return this;
    }
}