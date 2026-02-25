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
            new FileExportSettings(),
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
        var job = new FileTransferJob()
        {
            Key = "jottacloud_timeline",
            SourceDirectoryPath = @"C:\timeline",
            TargetDirectoryPath = @"C:\backup"
        };
        var settings = new FileExportSettings()
        { 
            FileTransferJobs = new List<FileTransferJob>() {
                job
            }
        };

        var duckSource = Path.Combine(
            job.SourceDirectoryPath,
            "2025",
            "02",
            "21",
            Path.GetFileName(TestData.Duck));
        var duckTarget = Path.Combine( // TODO: Fix path when target-dir from metadata is fixed
            job.TargetDirectoryPath,
            "2026",
            "02 Februar",
            Path.GetFileName(TestData.Duck));
        var duckContent = await File.ReadAllBytesAsync(TestData.Duck);
        
        var waterfallSource = Path.Combine(
            job.SourceDirectoryPath,
            "lorem ipsum",
            "dolor lamet",
            Path.GetFileName(TestData.Waterfall));
        var waterfallTarget = Path.Combine( // TODO: Fix path when target-dir from metadata is fixed
            job.TargetDirectoryPath,
            "2026",
            "02 Februar",
            Path.GetFileName(TestData.Waterfall));
        var waterfallContent = await File.ReadAllBytesAsync(TestData.Waterfall);

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { job.SourceDirectoryPath, new MockDirectoryData() },
            { duckSource, new MockFileData(duckContent) },
            { waterfallSource, new MockFileData(waterfallContent) },

        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var timelineExportService = new FileExportOrchestrator(
            settings,
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            fileStorageService,
            new Mock<IGooglePhotosRepository>().Object,
            new Mock<ISteamRepository>().Object);

        var result = await timelineExportService.ExportJottacloudTimelineAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(fileSystemMock.File.Exists(duckTarget));
        Assert.True(fileSystemMock.File.Exists(waterfallTarget));
    }
}
