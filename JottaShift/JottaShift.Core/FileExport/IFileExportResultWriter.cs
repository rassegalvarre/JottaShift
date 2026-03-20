using JottaShift.Core.FileExport.Jobs;

namespace JottaShift.Core.FileExport;

public interface IFileExportResultWriter
{
    /// <returns>A <see cref="Result{T}"/> containing the path the job result file</returns>
    Task<Result<string>> SaveFileTransferResult(
        FileTransferJob job, FileTransferJobResult jobResult, CancellationToken ct = default);
}
