using JottaShift.Core.FileStorage;
using JottaShift.Core.TimelineExport;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests;

public class TimelineExportTests
{
    [Fact]
    public void GetFullFileName_CreatesPathBasedOnFileCreationTime()
    {
        var creationDate = new DateTime(2026, 5, 31);
        string destinationDirectory = AppContext.BaseDirectory;
        string fileName = Path.GetRandomFileName();

        var timelineExportService = new TimelineExportService(
            new Mock<ILogger<TimelineExportService>>().Object,
            new Mock<IFileStorage>().Object);

        var fullFileName = timelineExportService.GetFullFileName(destinationDirectory, fileName, creationDate);
        var expectedFullFileName = Path.Combine(destinationDirectory, "2026", "05 Mai", fileName);
        Assert.Equal(expectedFullFileName, fullFileName);
    }
}
