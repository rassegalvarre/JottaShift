namespace JottaShift.Core.FileExportOrchestrator;

public enum FileExportJobStatus
{
    Diabled,
    Invalid,
    Ready,
    InProgress,
    Completed,
    Failed,
}

// TODO: Rename to FileExportJobResult and make abstract
// with derived classes for FileTransferJobResult and GooglePhotosUploadJobResult
public record FileExportJobResult(string Key)
{
    public string Key { get; init; } = Key;
    public FileExportJobStatus Status { get; private set; } = FileExportJobStatus.Ready;
    public string? SourceDirectoryPath { get; private set; }
    public string? TargetDirectoryPath { get; private set; }
    public string? ErrorMessage { get; private set; }
    public List<FileExportOperationResult> FileTransferOperationResults { get; init; } = [];

    private FileExportOperationResult? CurrentOperation {  get; set; }

    public bool Success => Status == FileExportJobStatus.Completed;
    public bool PreValidationFailed => Status == FileExportJobStatus.Invalid;

    private static FileExportJobResult CreateFromStatus(string key, FileExportJobStatus status)
    {
        return new FileExportJobResult(key)
        {
            Status = status
        };
    }

    public static FileExportJobResult Disabled(string jobKey)
    {
        return CreateFromStatus(jobKey, FileExportJobStatus.Diabled);
    }

    public static FileExportJobResult Invalid(string jobKey, string errorMessage)
    {
        return CreateFromStatus(jobKey, FileExportJobStatus.Invalid) with
        {
            ErrorMessage = errorMessage
        };
    }

    public static FileExportJobResult Ready(FileExportJob job)
    {
        return CreateFromStatus(job.Key, FileExportJobStatus.Ready) with
        {
            SourceDirectoryPath = job.SourceDirectoryPath,
            //TargetDirectoryPath = job.TargetDirectoryPath
        };
    }

    public static FileExportJobResult StartJob(FileExportJob job)
    {
        return CreateFromStatus(job.Key, FileExportJobStatus.InProgress) with
        {
            SourceDirectoryPath =  job.SourceDirectoryPath,
            //TargetDirectoryPath = job.TargetDirectoryPath
        };
    }

    public void PrepareOperation(string sourceFilePath)
    {
        if (CurrentOperation != null)
        {
            throw new InvalidOperationException("Cannot prepare a new operation while another operation is in progress.");
        }

        var operation = FileExportOperationResult.Prepare(sourceFilePath);
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

    public FileExportJobResult FailOperation(string errorMessage)
    {
        if (CurrentOperation != null)
        {
            Status = FileExportJobStatus.Failed;
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

    public FileExportJobResult CompleteJob()
    {
        if (CurrentOperation != null &&
            CurrentOperation.Status != FileExportOperationResultStatus.Completed)
        {
            throw new InvalidOperationException("Current operation as not been marked as completed.");
        }
        
        Status = FileExportJobStatus.Completed;
        return this;
    }
}