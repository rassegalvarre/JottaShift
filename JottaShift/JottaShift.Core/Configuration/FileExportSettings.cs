using System.Text.Json.Serialization;

namespace JottaShift.Core.Configuration;

public class FileExportSettings
{
    [JsonPropertyName("file_transfer_jobs")]
    public IEnumerable <FileTransfer> FileTransferJobs { get; init; } = [];

    [JsonPropertyName("google_photos_upload_jobs")]
    public IEnumerable<GooglePhotosUpload> GooglePhotosUploadJobs { get; init; } = [];
}

public abstract record FileExportJob()
{
    [JsonPropertyName("job_key")]
    public required string JobKey { get; init; }

    [JsonPropertyName("source_directory_path")]
    public required string SourceDirectoryPath { get; init; }
}

public record FileTransfer : FileExportJob
{
    [JsonPropertyName("target_directory_path")]
    public required string TargetDirectoryPath { get; init; }
}

public record GooglePhotosUpload : FileExportJob
{
    [JsonPropertyName("album_name")]
    public required string AlbumName { get; init; }
}