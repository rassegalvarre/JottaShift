namespace JottaShift.Core.FileExportOrchestrator;

public record GooglePhotosUploadOperationResult(
    bool Success,
    string SourceFilePath,
    bool SourceFileDeleted);

public class GooglePhotosUploadJobResult
{
    public required bool Success { get; init; }
    public required string AlbumName { get; init; }
    public required IEnumerable<GooglePhotosUploadOperationResult> GooglePhotosUploadOperationResults { get; init; }

    public GooglePhotosUploadJobResult(bool success, string albumName)
    {
        Success = success;
        AlbumName = albumName;
        GooglePhotosUploadOperationResults = [];
    }

    public GooglePhotosUploadJobResult(
        bool success,
        string albumName,
        IEnumerable<GooglePhotosUploadOperationResult> googlePhotosUploadOperationResults)
    {
        Success = success;
        AlbumName = albumName;
        GooglePhotosUploadOperationResults = googlePhotosUploadOperationResults;
    }
}