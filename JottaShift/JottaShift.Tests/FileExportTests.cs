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
            "2025",
            "05 Mai",
            Path.GetFileName(TestData.Duck));
        var duckContent = await File.ReadAllBytesAsync(TestData.Duck);

        var waterfallSource = Path.Combine(
            job.SourceDirectoryPath,
            "lorem ipsum",
            "dolor lamet",
            Path.GetFileName(TestData.Waterfall));
        var waterfallTarget = Path.Combine( // TODO: Fix path when target-dir from metadata is fixed
            job.TargetDirectoryPath,
            "2025",
            "05 Mai",
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

    [Fact]
    public async Task ExportSteamScreenshotsAsync_ShouldExportSteamScreenshots_ToDirectoryWithAppName()
    {

        var appIdAndNamePair = new Dictionary<uint, string>
        {
            { 1, "Pung" },
            { 12345, "Duum" },
            { 987653, "Super Mawio" }        
        };

        var jobSettings = new FileTransferJob()
        {
            Key = "steam_screenshots",
            SourceDirectoryPath = @"C:\steam",
            TargetDirectoryPath = @"C:\backup\steam"
        };

        var mockFileData = new Dictionary<string, MockFileData>();
        var steamRepositoryMock = new Mock<ISteamRepository>();

        var fileByteContent = File.ReadAllBytes(TestData.Duck);
        foreach (var app in appIdAndNamePair)
        {
            mockFileData.Add(
                Path.Combine(@"C:\steam", app.Key.ToString(), "image.png"),
                new MockFileData(fileByteContent));
        
            steamRepositoryMock
                .Setup(repo => repo.GetAppNameFromId(app.Key))
                .ReturnsAsync(app.Value);        
        }

        var fileSystemMock = new MockFileSystem(mockFileData);

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);


        var fileExportOrchestrator = new FileExportOrchestrator(
            new FileExportSettings()
            {
                FileTransferJobs = new List<FileTransferJob>()
                {
                    jobSettings
                }
            },
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            fileStorageService,
            new Mock<IGooglePhotosRepository>().Object,
            steamRepositoryMock.Object);

        var result = await fileExportOrchestrator.ExportSteamScreenshotsAsync();

        Assert.True(result.Success);

        foreach (var app in appIdAndNamePair)
        {
            var expectedTargetPath = Path.Combine(jobSettings.TargetDirectoryPath, app.Value, "image.png");
            Assert.True(fileSystemMock.File.Exists(expectedTargetPath), $"Expected file at path {expectedTargetPath} was not found.");
        }
    }
}