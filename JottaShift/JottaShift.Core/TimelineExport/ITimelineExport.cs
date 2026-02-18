namespace JottaShift.Core.TimelineExport;

public interface ITimelineExport
{
    Task<TimelineExportResult> ExportAsync(TimelineExportOptions options, CancellationToken ct = default);
}
