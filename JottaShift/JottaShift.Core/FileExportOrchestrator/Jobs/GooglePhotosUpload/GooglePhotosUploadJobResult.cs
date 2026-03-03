namespace JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;

/// <summary>
/// Result for <see cref="GooglePhotosUploadJob"/> execution"/>
/// </summary>
public record GooglePhotosUploadJobResult(string Key) : FileExportJobResult(Key)
{
    public string? AlbumName { get; init; }
    public int PhotosUploaded { get; private set; } = 0;

    public static GooglePhotosUploadJobResult CreateFromJob(GooglePhotosUploadJob job)
    {
        return new GooglePhotosUploadJobResult(job.Key)
        {
            Key = job.Key,
            SourceDirectoryPath = job.SourceDirectoryPath,
            AlbumName = job.AlbumName
        };
    }

    public override GooglePhotosUploadJobResult Complete()
    {
        base.Complete();                
        return this;
    }

    public override GooglePhotosUploadJobResult Fail(string errorMessage)
    {
        base.Fail(errorMessage);
        return this;
    }
}
