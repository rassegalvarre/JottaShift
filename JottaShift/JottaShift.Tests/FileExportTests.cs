using JottaShift.Core.FileExportOrchestrator;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;

namespace JottaShift.Tests;

public class FileExportTests
{
    [Fact]
    public void GetTargetDirectoryNameFromFileTimestamp_CreatesPathBasedOnFileCreationTime()
    {
        var creationDate = new DateTime(2026, 5, 31);
        string destinationDirectory = AppContext.BaseDirectory;
        string fileName = Path.GetRandomFileName();

        var timelineExportService = new FileExportOrchestrator(
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            new Mock<IFileStorage>().Object,
            new Mock<IGooglePhotosRepository>().Object,
            new Mock<ISteamRepository>().Object);

        var fullFileName = timelineExportService.GetTargetDirectoryNameFromFileTimestamp(destinationDirectory, fileName, creationDate);
        var expectedDirectoryName = Path.Combine(destinationDirectory, "2026", "05 Mai");
        Assert.Equal(expectedDirectoryName, fullFileName);
    }

    [Fact]
    public async Task ExportAsync_ShouldExportAndRestrucutureTimeline()
    {
        string sourceDirectory = @"C:\timeline";
        string destinationDirectory = @"C:\backup";

        string img2025_12_31_source = Path.Combine(sourceDirectory, "img20251231.jpg");
        string img2025_12_31_destination = @"C:\backup\2025\12 Desember\img20251231.jpg";
        var img2025_12_31_data = new MockFileData([])
        {
            LastWriteTime = new DateTime(2025, 12, 31, 23, 0, 0)
        };

        string img2026_01_01_source = Path.Combine(sourceDirectory, "2026", "1", "2", "img20260101.jpg");
        string img2026_01_01_destination = @"C:\backup\2026\01 Januar\img20260101.jpg";
        var img2026_01_01_data = new MockFileData([])
        {
            LastWriteTime = new DateTime(2026, 1, 1, 1, 0, 0)
        };

        string img2026_01_31_source = Path.Combine(sourceDirectory, "2026", "2", "3", "img20260131.jpg");
        string img2026_01_31_destination = @"C:\backup\2026\01 Januar\img20260131.jpg";
        var img2026_01_31_data = new MockFileData([])
        {
            LastWriteTime = new DateTime(2026, 1, 31, 12, 0, 0)
        };

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { sourceDirectory, new MockDirectoryData() },
            { img2025_12_31_source, img2025_12_31_data },
            { img2026_01_01_source, img2026_01_01_data },
            { img2026_01_31_source, img2026_01_31_data },

        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var timelineExportService = new FileExportOrchestrator(
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            fileStorageService,
            new Mock<IGooglePhotosRepository>().Object,
            new Mock<ISteamRepository>().Object);

        var options = new FileExportOptions(sourceDirectory, destinationDirectory);
        var result = await timelineExportService.ExportJottacloudTimelineAsync(options, new CancellationToken());

        Assert.True(result.Success);
        Assert.True(fileSystemMock.File.Exists(img2025_12_31_destination));
        Assert.True(fileSystemMock.File.Exists(img2026_01_01_destination));
        Assert.True(fileSystemMock.File.Exists(img2026_01_31_destination));
    }
}
