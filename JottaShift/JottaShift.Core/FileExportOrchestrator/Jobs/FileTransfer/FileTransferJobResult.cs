namespace JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;

/// <summary>
/// Result for <see cref="FileTransferJob"/> execution"/>
/// </summary>
public record FileTransferJobResult(string Key) : FileExportJobResult(Key)
{
    public string? TargetDirectoryPath { get; init; }

    public List<FileTransferOperationResult> Operations { get; init; } = [];

    private FileTransferOperationResult? CurrentOperation { get; set; }

    public static FileTransferJobResult CreateFromJob(FileTransferJob job)
    {
        return new FileTransferJobResult(job.Key)
        {
            Key = job.Key,
            SourceDirectoryPath = job.SourceDirectoryPath,
            TargetDirectoryPath = job.TargetDirectoryPath
        };
    }

    public override FileTransferJobResult Complete()
    {
        if (CurrentOperation != null &&
            CurrentOperation.Status != FileTransferOperationStatus.Completed)
        {
            throw new InvalidOperationException("Current operation as not been marked as completed.");
        }

        base.Complete();
        return this;
    }

    public override FileTransferJobResult Fail(string errorMessage)
    {
        base.Fail(errorMessage);
        return this;
    }

    public void PrepareOperation(string sourceFilePath)
    {
        if (CurrentOperation != null)
        {
            throw new InvalidOperationException("Cannot prepare a new operation while another operation is in progress.");
        }

        CurrentOperation = new FileTransferOperationResult(sourceFilePath);
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
            base.Fail(errorMessage);

            CurrentOperation.Fail(errorMessage);
            Operations.Add(CurrentOperation);

            CurrentOperation = null;
            return this;
        }

        throw new InvalidOperationException("No current operation to fail");
    }

    public void CompleteOperation(string targetFilePath)
    {
        if (CurrentOperation == null)
        {
            throw new InvalidOperationException("No current operation to complete.");
        }

        CurrentOperation.Complete(targetFilePath);
        Operations.Add(CurrentOperation);
        CurrentOperation = null;
    }
}