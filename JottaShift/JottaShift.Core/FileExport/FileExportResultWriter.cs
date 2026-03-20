using JottaShift.Core.FileExport.Jobs;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;

namespace JottaShift.Core.FileExport;

public class FileExportResultWriter(
    IFileStorageService _fileStorageService,
    IFileWriterFactory _fileWriterFactory) : IFileExportResultWriter
{
    public async Task<Result<string>> SaveChromecastUploadResult(
        ChromecastUploadJob job, AlbumUploadResult jobResult, CancellationToken ct = default)
    {
        var timestamp = DateTime.Now;
        string outputFileName = Path.Combine(AppContext.BaseDirectory, "Jobs", $"{job.Id}_{timestamp:yyyyMMdd_HHmmss}.txt");

        var directory = Path.GetDirectoryName(outputFileName);
        if (string.IsNullOrEmpty(directory))
        {
            return Result<string>.Failure("Could not determine output directory.");
        }

        if (!_fileStorageService.ValidateDirectory(directory).Succeeded)
        {
            return Result<string>.Failure("Could not validate output directory.");
        }

        using var writer = _fileWriterFactory.CreateFileWriter(outputFileName);

        await writer.WriteAsync(
            $"[{job.Id}]\n" +
            $"Finished:                     {timestamp:yyyy-MM-dd HH:mm:ss}\n" +
            $"Succeeded:                    {jobResult.Succeeded}\n" +
            $"Error message:                {jobResult.ErrorMessage ?? "No errors"}\n" +
            $"Source Jottacloud album:      {job.SourceJottacloudAlbumId}\n" +
            $"Target Google Photos album:   {job.TargetGooglePhotosAlbumName}\n\n");

        if (jobResult.PhotoUploadResults == null || !jobResult.PhotoUploadResults.Any())
        {
            await writer.WriteLineAsync(
                "[Photos uploaded]" +
                "No photos were uploaded in this job.");
            return Result<string>.Success(outputFileName);
        }

        var successfulUploads = jobResult.PhotoUploadResults.Where(r => r.StatusMessage == "Success");
        var unsuccessfulUploads = jobResult.PhotoUploadResults.Where(r => r.StatusMessage != "Success");

        await writer.WriteLineAsync(
            "[Upload summary]\n" +
            $"Succeeded:    {successfulUploads.Count()}\n" +
            $"Failed:       {unsuccessfulUploads.Count()}\n");

        if (unsuccessfulUploads.Any())
        {
            await writer.WriteLineAsync("[Failed]");
            await WritePhotoUploadResults(writer, unsuccessfulUploads);
        }

        if (successfulUploads.Any())
        {
            await writer.WriteLineAsync("[Succeeded]");
            await WritePhotoUploadResults(writer, successfulUploads);

        }

        return Result<string>.Success(outputFileName);
    }

    public async Task<Result<string>> SaveFileTransferResult(
        FileTransferJob job, FileTransferJobResult jobResult, CancellationToken ct = default)
    {
        var timestamp = DateTime.Now;
        string outputFileName = Path.Combine(AppContext.BaseDirectory, "Jobs", $"{job.Id}_{timestamp:yyyyMMdd_HHmmss}.txt");

        var directory = Path.GetDirectoryName(outputFileName);
        if (string.IsNullOrEmpty(directory))
        {
            return Result<string>.Failure("Could not determine output directory.");
        }

        if (!_fileStorageService.ValidateDirectory(directory).Succeeded)
        {
            return Result<string>.Failure("Could not validate output directory.");
        }

        using var writer = _fileWriterFactory.CreateFileWriter(outputFileName);

        await writer.WriteAsync(
            $"[{job.Id}]\n" +
            $"Finished:                 {timestamp:yyyy-MM-dd HH:mm:ss}\n" +
            $"Succeeded:                {jobResult.Succeeded}\n" +
            $"Status:                   {jobResult.Status}\n" +
            $"Error message:            {jobResult.ErrorMessage ?? "No errors"}\n" +
            $"Source directory:         {job.SourceDirectoryPath}\n" +
            $"Target directory:         {job.TargetDirectoryPath}\n" +
            $"Delete source enabled:    {job.DeleteSourceFiles}\n" +
            $"Source deleted:           {jobResult.SourceDirectoryDeleted}\n\n");

        if (jobResult.Value == null || !jobResult.Value.Any())
        {
            await writer.WriteLineAsync(
                "[Files processed]" +
                "No files were processed in this job.");
            return Result<string>.Success(outputFileName);
        }

        var successfulTransfers = jobResult.Value.Where(v => v.Succeeded);
        var unsuccessfulTransfers = jobResult.Value.Where(v => !v.Succeeded);

        await writer.WriteLineAsync(
            "[Transfer summary]\n" +
            $"Succeeded:    {successfulTransfers.Count()}\n" +
            $"Failed:       {unsuccessfulTransfers.Count()}\n");

        if (unsuccessfulTransfers.Any())
        {
            await writer.WriteLineAsync("[Failed]");
            await WriteFileTransferResults(writer, unsuccessfulTransfers);
        }

        if (successfulTransfers.Any())
        {
            await writer.WriteLineAsync("[Succeeded]");
            await WriteFileTransferResults(writer, successfulTransfers);

        }

        return Result<string>.Success(outputFileName);
    }

    private async Task WriteFileTransferResults(IFileWriter writer, IEnumerable<FileTransferResult> results)
    {
        foreach (var result in results)
        {
            await writer.WriteAsync(
                $"Source:           {result.SourceFileFullPath}\n" +
                $"Target:           {result.NewFileFullPath ?? "No target"}\n" +
                $"Source deleted:   {result.SourceFileDeleted}\n");

            if (!result.Succeeded)
            {
                await writer.WriteAsync(
                    $"Status:           {result.Status}\n" +
                    $"Error message:    {result.ErrorMessage}\n");
            }

            await writer.WriteAsync("\n");
        }
    }

    private async Task WritePhotoUploadResults(IFileWriter writer, IEnumerable<PhotoUploadResult> results)
    {
        foreach (var result in results)
        {
            await writer.WriteAsync(
                $"Local file path:  {result.FilePath}\n" +
                $"Url:              {result.Url ?? "No url"}\n");

            await writer.WriteAsync("\n");
        }
    }
}
