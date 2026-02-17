namespace JottaShift.Core.TimelineExport;

public interface ITimelineExport
{
    Task ExportAsync(TimelineExportOptions options, CancellationToken ct = default);
}
