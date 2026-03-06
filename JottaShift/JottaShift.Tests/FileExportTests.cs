using JottaShift.Core.FileExportOrchestrator;
using JottaShift.Core.FileExportOrchestrator.Jobs;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace JottaShift.Tests;

public class FileExportTests(FileExportFixture _fixture) : IClassFixture<FileExportFixture>
{
    public static List<object[]> GetAlphabeticParentDirectoryNameTestData() => new()
    {
        new[] { "?Æ:;(", "0 - Numerisk" },
        new[] { "007", "0 - Numerisk" },
        new[] { "Apple","A - Alpha" },
        new[] { "Banana", "B - Bravo" },
        new[] { "Cherry", "C - Charlie" },
        new[] { "Date", "D - Delta" },
        new[] { "Elderberry", "E - Echo" },
        new[] { "Fig", "F - Foxtrot" },
        new[] { "Grape", "G - Golf" },
        new[] { "Honeydew", "H - Hotel" },
        new[] { "Iceberg lettuce" ,"I - India"},
        new[] { "Jackfruit", "J - Juliett" },
        new[] { "Kiwi", "K - Kilo" },
        new[] { "Lemon","L - Lima" },
        new[] { "Mango", "M - Mike" },
        new[] { "Nectarine", "N - November"},
        new[] { "Orange", "O - Oscar" },
        new[] { "Papaya", "P - Papa" },
        new[] { "Quince", "Q - Quebec" },
        new[] { "Raspberry", "R - Romeo" },
        new[] { "Strawberry","S - Sierra" },
        new[] { "Tomato", "T - Tango" },
        new[] { "Ugli fruit", "U - Uniform" },
        new[] { "Vanilla bean", "V - Victor"},
        new[] { "Watermelon", "W - Whiskey" },
        new[] { "Xigua", "X - X‑ray" },
        new[] { "Yellow pepper","Y - Yankee"},
        new[] { "Zucchini",  "Z - Zulu" }
    };

    [Theory]
    [MemberData(nameof(GetAlphabeticParentDirectoryNameTestData))]
    public void GetAlphabeticParentDirectoryName(string directoryName, string expected)
    {
        var orchestrator = _fixture.CreateFileExportOrchestrator();

        var alphabeticParentDirectoryName = orchestrator.GetAlphabeticParentDirectoryName(directoryName);

        Assert.Equal(expected, alphabeticParentDirectoryName);
    }

    [Fact]
    public void GetTargetDirectoryNameFromFileTimestamp_CreatesPathBasedOnFileCreationTime()
    {
        var creationDate = new DateTime(2026, 5, 31);
        string destinationDirectory = AppContext.BaseDirectory;

        var timelineExportService = _fixture.CreateFileExportOrchestrator();

        var culture = CultureInfo.GetCultureInfo("en-GB");
        timelineExportService.SetCulture(culture);

        string directoryNameResult = timelineExportService.GetTargetDirectoryNameFromFileTimestamp(destinationDirectory, creationDate);
        string expectedDirectoryName = Path.Combine(destinationDirectory, "2026", "05 May");

        Assert.Equal(expectedDirectoryName, directoryNameResult);
    }

    [Fact]
    public async Task ExportAsync_ShouldExportAndRestrucutureTimeline()
    {
        var job = _fixture.JottacloudTimelineJob;
        var duckSource = Path.Combine(
            job.SourceDirectoryPath,
            "2025",
            "02",
            "21",
            Path.GetFileName(TestData.Duck));
        var duckTarget = Path.Combine(
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
        var waterfallTarget = Path.Combine(
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

        var timelineExportService = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorageService);

        var culture = CultureInfo.GetCultureInfo("en-GB");
        timelineExportService.SetCulture(culture);

        var result = await timelineExportService.ExportJottacloudTimelineAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(fileSystemMock.File.Exists(duckTarget));
        Assert.True(fileSystemMock.File.Exists(waterfallTarget));
        Assert.False(fileSystemMock.File.Exists(duckSource));
        Assert.False(fileSystemMock.File.Exists(waterfallSource));
    }

    [Theory]
    [InlineData(1, "Pung", "P - Papa")]
    [InlineData(12345, "Duum", "D - Delta")]
    [InlineData(987653, "Super Mawio", "S - Sierra")]
    public async Task ExportSteamScreenshotsAsync_ShouldExportSteamScreenshots_ToDirectoryWithAppName(
        uint appId,
        string appName,
        string parentDirectoryName)
    {
        var jobSettings = _fixture.SteamScreenshotsJob;

        var mockFileData = new Dictionary<string, MockFileData>();
        var steamRepositoryMock = new Mock<ISteamRepository>();

        var fileByteContent = File.ReadAllBytes(TestData.Duck);
        string sourceFilePath = Path.Combine(jobSettings.SourceDirectoryPath, appId.ToString(), "image.png");
        mockFileData.Add(sourceFilePath, new MockFileData(fileByteContent));

        steamRepositoryMock
            .Setup(repo => repo.GetAppNameFromId(appId))
            .ReturnsAsync(appName);

        var fileSystemMock = new MockFileSystem(mockFileData);

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorageService,
            steamRepository: steamRepositoryMock.Object);

        var result = await fileExportOrchestrator.ExportSteamScreenshotsAsync();
        var expectedTargetPath = Path.Combine(jobSettings.TargetDirectoryPath, parentDirectoryName, appName, "image.png");

        Assert.True(result.Success);
        Assert.False(fileSystemMock.File.Exists(sourceFilePath));
        Assert.True(fileSystemMock.File.Exists(expectedTargetPath),
            $"Expected file at path {expectedTargetPath} was not found.");
    }

    [Fact]
    [Trait("API", "Google")]
    public async Task ExportChromecastPhotosAsync_ShouldExportPhots_ToAlbumName()
    {
        var fileSystem = new FileSystem();
        var fileStorageService = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);
        var googlePhotosRepository = new GooglePhotosRepository(
            new Mock<ILogger<GooglePhotosRepository>>().Object,
            fileSystem);

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorageService,
            googlePhotosRepository: googlePhotosRepository);

        var result = await fileExportOrchestrator.ExportChromecastPhotosAsync();

        Assert.True(result.Success);
        // Assert.Equal(2, result.GooglePhotosUploadOperationResults.Count());
    }

    [Fact]
    public async Task ExportDesktopWallpapersAsync_ShouldExportWallpapers_ToDirectoryBasedOnResolution()
    {
        var imageBytes = await File.ReadAllBytesAsync(TestData.Egypt);

        var job = _fixture.DesktopWallpapersJob;
        string sourceFilePath = Path.Combine(job.SourceDirectoryPath, Path.GetFileName(TestData.Egypt));
        string expectedTargetPath = Path.Combine(job.TargetDirectoryPath, "4K", "egypt.jpg");

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { job.SourceDirectoryPath, new MockDirectoryData() },
            { job.TargetDirectoryPath, new MockDirectoryData() },
            { sourceFilePath, new MockFileData(imageBytes) },
        });

        var fileStorageService = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(fileStorage: fileStorageService);

        var result = await fileExportOrchestrator.ExportDesktopWallpapersAsync();
        var operation = result.Operations.FirstOrDefault();
        Assert.True(result.Success);
        Assert.True(result.Operations.Count > 0);
        Assert.True(operation?.Success == true);
        Assert.Equal(expectedTargetPath, operation.TargetFilePath);
        Assert.True(fileSystem.File.Exists(expectedTargetPath));
        Assert.False(fileSystem.File.Exists(sourceFilePath));
    }
}