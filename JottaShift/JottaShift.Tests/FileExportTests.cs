using JottaShift.Core.FileExportOrchestrator;
using JottaShift.Core.FileExportOrchestrator.Jobs;
using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;
using JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace JottaShift.Tests;

public class FileExportTests
{
    [Fact]
    public void GetTargetDirectoryNameFromFileTimestamp_CreatesPathBasedOnFileCreationTime()
    {
        var creationDate = new DateTime(2026, 5, 31);
        string destinationDirectory = AppContext.BaseDirectory;

        var timelineExportService = new FileExportOrchestrator(
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            new Mock<IFileStorage>().Object,
            new FileExportJobValidator(
                new Mock<ILogger<FileExportJobValidator>>().Object,
                new FileExportSettings(),
                new Mock<IFileStorage>().Object),
            new Mock<IGooglePhotosRepository>().Object,
            new Mock<ISteamRepository>().Object);

        var culture = CultureInfo.GetCultureInfo("en-GB");
        timelineExportService.SetCulture(culture);

        string directoryNameResult = timelineExportService.GetTargetDirectoryNameFromFileTimestamp(destinationDirectory, creationDate);
        string expectedDirectoryName = Path.Combine(destinationDirectory, "2026", "05 May");

        Assert.Equal(expectedDirectoryName, directoryNameResult);
    }

    [Fact]
    public async Task ExportAsync_ShouldExportAndRestrucutureTimeline()
    {
        var job = new FileTransferJob()
        {
            Key = "jottacloud_timeline",
            SourceDirectoryPath = @"C:\timeline",
            TargetDirectoryPath = @"C:\backup",
            Enabled = true,
            DeleteSourceFiles = true
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
            "05 May",
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
            "05 May",
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
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            fileStorageService,
            new FileExportJobValidator(
                new Mock<ILogger<FileExportJobValidator>>().Object,
                settings,
                fileStorageService),
            new Mock<IGooglePhotosRepository>().Object,
            new Mock<ISteamRepository>().Object);

        var culture = CultureInfo.GetCultureInfo("en-GB");
        timelineExportService.SetCulture(culture);

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
            TargetDirectoryPath = @"C:\backup\steam",
            Enabled = true,
            DeleteSourceFiles = true
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
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            fileStorageService,
            new FileExportJobValidator(
                new Mock<ILogger<FileExportJobValidator>>().Object,
                new FileExportSettings()
                {
                    FileTransferJobs = new List<FileTransferJob>()
                    {
                        jobSettings
                    }
                },
                fileStorageService),
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

    [Fact]
    [Trait("API", "Google")]
    public async Task ExportChromecastPhotosAsync_ShouldExportPhots_ToAlbumName()
    {

        var fileSystem = new FileSystem();
        var fileStorageService = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);
        var googlePhotosRepositoryMock = new GooglePhotosRepository(fileSystem);

        var fileExportOrchestrator = new FileExportOrchestrator(
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            fileStorageService,
            new FileExportJobValidator(
                new Mock<ILogger<FileExportJobValidator>>().Object,
                new FileExportSettings()
                {
                    GooglePhotosUploadJobs = new List<GooglePhotosUploadJob>()
                    {
                        new GooglePhotosUploadJob()
                        {
                            Key = "chromecast_photos",
                            SourceDirectoryPath = TestData.TestDataPath,
                            AlbumName = "JottaSync.UnitTests.FileExport",
                            Enabled = true,
                            DeleteSourceFiles = false
                        }
                    }
                },
                fileStorageService
            ),
            googlePhotosRepositoryMock,
            new Mock<ISteamRepository>().Object);

        var result = await fileExportOrchestrator.ExportChromecastPhotosAsync();

        Assert.True(result.Success);
        // Assert.Equal(2, result.GooglePhotosUploadOperationResults.Count());
    }

    [Fact]
    public async Task ExportDesktopWallpapersAsync_ShouldExportWallpapers_ToDirectoryBasedOnResolution()
    {
        var imageBytes = await File.ReadAllBytesAsync(TestData.Egypt);

        string sourceDirectory = @"C:\games";
        var sourceFilePath = Path.Combine(sourceDirectory, Path.GetFileName(TestData.Egypt));
        string targetDirectory = @"C:\wallpapers";
        string expectedTarget = Path.Combine(targetDirectory, "4K", "egypt.jpg");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { sourceDirectory, new MockDirectoryData() },
            { targetDirectory, new MockDirectoryData() },
            { sourceFilePath, new MockFileData(imageBytes) },
        });

        var fileStorageService = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);

        var fileExportOrchestrator = new FileExportOrchestrator(
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            fileStorageService,
            new FileExportJobValidator(
                new Mock<ILogger<FileExportJobValidator>>().Object,
                new FileExportSettings()
                {
                    FileTransferJobs = [
                        new FileTransferJob()
                        {
                            Key = "desktop_wallpapers",
                            SourceDirectoryPath = sourceDirectory,
                            TargetDirectoryPath = targetDirectory,
                            Enabled = true,
                            DeleteSourceFiles = false
                        }
                    ]
                },
                fileStorageService
            ),
            new Mock<IGooglePhotosRepository>().Object,
            new Mock<ISteamRepository>().Object);

        var result = await fileExportOrchestrator.ExportDesktopWallpapersAsync();
        var operation = result.Operations.FirstOrDefault();
        Assert.True(result.Success,
            "Result did not have status Success");
        Assert.True(result.Operations.Count > 0,
            "No operation in job was executed");
        Assert.True(operation?.Success == true,
            "Operation was not successfull");
        Assert.Equal(expectedTarget, operation.TargetFilePath);          
    }
}