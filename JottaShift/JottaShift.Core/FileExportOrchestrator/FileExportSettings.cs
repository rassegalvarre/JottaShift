using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;
using JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;
using System.Text.Json.Serialization;

namespace JottaShift.Core.FileExportOrchestrator;

public class FileExportSettings
{
    [JsonPropertyName("file_transfer_jobs")]
    public IEnumerable <FileTransferJob> FileTransferJobs { get; init; } = [];

    [JsonPropertyName("google_photos_upload_jobs")]
    public IEnumerable<GooglePhotosUploadJob> GooglePhotosUploadJobs { get; init; } = [];
}