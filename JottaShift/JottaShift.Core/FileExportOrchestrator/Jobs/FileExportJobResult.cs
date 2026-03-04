namespace JottaShift.Core.FileExportOrchestrator.Jobs;

/// <summary>
/// Result for <see cref="FileExportJob"/> execution"/>
/// </summary>
public abstract record FileExportJobResult(string Key)
{
    public string Key { get; init; } = Key;

    public FileExportJobStatus Status { get; private set; } = FileExportJobStatus.Ready;

    public string SourceDirectoryPath { get; protected set; } = string.Empty;
    
    public string? ErrorMessage { get; private set; }
    
    public bool Success => Status == FileExportJobStatus.Completed;
    
    public bool PreValidationFailed => Status == FileExportJobStatus.Invalid;

    public void Disabled()
    {
        Status = FileExportJobStatus.Disabled;
        ErrorMessage = "Job is disabled and will not be executed.";
    }

    public void Invalid()
    {
        Status = FileExportJobStatus.Invalid;
        ErrorMessage = "Job failed pre-validation cannot be executed.";
    }

    public void Ready<T>(T job) where T : FileExportJob
    {
        Status = FileExportJobStatus.Ready;
        SourceDirectoryPath = job.SourceDirectoryPath;
    }

    public void Start()
    {
        Status = FileExportJobStatus.InProgress;
    }

    public virtual FileExportJobResult Complete()
    {
        Status = FileExportJobStatus.Completed;
        return this;
    }

    public virtual FileExportJobResult Fail(string errorMessage)
    {
        Status = FileExportJobStatus.Failed;
        ErrorMessage = errorMessage;
        return this;
    }
}