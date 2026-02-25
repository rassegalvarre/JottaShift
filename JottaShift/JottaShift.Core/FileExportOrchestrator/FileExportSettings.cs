using System.Text.Json.Serialization;

namespace JottaShift.Core.FileExportOrchestrator;

public class FileExportSettings
{
    [JsonPropertyName("file_transfer_jobs")]
    public IEnumerable <FileTransferJob> FileTransferJobs { get; init; } = [];

    [JsonPropertyName("google_photos_upload_jobs")]
    public IEnumerable<GooglePhotosUploadJob> GooglePhotosUploadJobs { get; init; } = [];
}

public abstract record FileExportJob()
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("source_directory_path")]
    public required string SourceDirectoryPath { get; init; }
}

public record FileTransferJob : FileExportJob
{
    [JsonPropertyName("target_directory_path")]
    public required string TargetDirectoryPath { get; init; }
}

public record GooglePhotosUploadJob : FileExportJob
{
    [JsonPropertyName("album_name")]
    public required string AlbumName { get; init; }
}