using JottaShift.Core;
using JottaShift.Core.FileExport;
using JottaShift.Core.FileStorage;
using JottaShift.Core.Steam;
using JottaShift.Tests.GooglePhotos;
using JottaShift.Tests.TestData;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO.Abstractions.TestingHelpers;

namespace JottaShift.Tests.FileExport;

public class FileExportOrchestratorTests(
    FileExportFixture _fixture,
    GooglePhotosFixture _googlePhotosFixture
    ) : IClassFixture<FileExportFixture>, 
        IClassFixture<GooglePhotosFixture>
{
    #region GetAlphabeticParentDirectoryName
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
    #endregion

    #region GetDirectoryNameForImageResolution
    [Theory]
    [InlineData("2160", "4K")]
    [InlineData("3840x2160", "4K")]
    [InlineData("5120x1440", "QHD")]
    [InlineData("3440x1440", "QHD")]
    [InlineData("2560x1440", "QHD")]
    [InlineData("2560x1080", "FullHD")]
    [InlineData("1920x1080", "FullHD")]
    public void GetDirectoryNameForImageResolution_ShouldReturnCorrectDirectoryName(string resolution, string expectedDirectory)
    {
        var directoryNameResult = FileExportOrchestrator.GetDirectoryNameForImageResolution(resolution);

        ResultAssert.ValueSuccess(directoryNameResult, expectedDirectory);
    }

    [Theory]
    [InlineData("")]
    [InlineData("someString")]
    [InlineData("1001")]
    [InlineData("2560x1600")]
    [InlineData("2880x1920")]
    public void GetDirectoryNameForImageResolution_ShouldReturnFailure_WhenInvalidResolution(string resolution)
    {
        var directoryNameResult = FileExportOrchestrator.GetDirectoryNameForImageResolution(resolution);

        ResultAssert.ValueFailure(directoryNameResult);
    }

    #endregion

    #region ExportJottacloudTimeline
    // Test-paths need to match patch defined in FileExportFixture.DefaultJobs
    [Theory]
    [InlineData(
        @"C:\source\timeline\img_20201201.jpg", 
        @"C:\backup\timeline\2020\12 December\img_20201201.jpg")]
    [InlineData(
        @"C:\source\timeline\img_20201201(Conflict 2026-03-16).jpg",
        @"C:\backup\timeline\2020\12 December\img_20201201.jpg")]
    [InlineData(
        @"C:\source\timeline\images(Conflict 2026-03-16)\img_20201201.jpg",
        @"C:\backup\timeline\2020\12 December\img_20201201.jpg")]
    [InlineData(
        @"C:\source\timeline\videos\2020\vid_20201201.mp4", 
        @"C:\backup\timeline\2020\12 December\vid_20201201.mp4")]
    public async Task ExportJottacloudTimeline_ShouldCopySingleFileWithStructuredPath(
        string sourceFilePath,
        string expectedFilePath)
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { sourceFilePath, new MockFileData([]) }
        });
        var fileStorage = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);
        
        var orchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorage);

        var result = await orchestrator.ExportJottacloudTimelineAsync(CancellationToken.None);

        // Assert
        ResultAssert.Success(result);

        Assert.False(fileSystem.File.Exists(sourceFilePath));
        Assert.True(fileSystem.File.Exists(expectedFilePath));
        Assert.Empty(fileSystem.Directory.EnumerateFileSystemEntries(
            _fixture.DefaultFileExportJobs.JottacloudTimelineExportJob.SourceDirectoryPath));
    }

    [Fact]
    public async Task ExportAsync_ShoulNotDeleteSourceDirectoryContent_WhenOneCopyFails()
    {
        var job = _fixture.DefaultFileExportJobs.JottacloudTimelineExportJob;

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { Path.Combine(job.SourceDirectoryPath, "img_20200101.jpg"), new MockFileData([]) },
            { Path.Combine(job.SourceDirectoryPath, "sub-dir", "vid_20210612.mp4"), new MockFileData([]) },
            { Path.Combine(job.SourceDirectoryPath, "sub-dir", "<invalid-name"), new MockFileData([]) }
        });
        var fileStorage = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);

        var orchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorage);

        var result = await orchestrator.ExportJottacloudTimelineAsync(CancellationToken.None);

        // Assert
        ResultAssert.Success(result);

        Assert.NotEmpty(fileSystem.Directory.EnumerateFileSystemEntries(
            _fixture.DefaultFileExportJobs.JottacloudTimelineExportJob.SourceDirectoryPath));
    }

    [Fact]
    public async Task ExportAsync_ShouldReturnSuccessAndCreateDirectories_WhenNotExists()
    {
        var job = _fixture.DefaultFileExportJobs.JottacloudTimelineExportJob;
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

        var fileStorage = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);

        var orchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorage);

        var result = await orchestrator.ExportJottacloudTimelineAsync(CancellationToken.None);

        // Assert
        ResultAssert.Success(result);
        Assert.True(fileSystem.Directory.Exists(job.SourceDirectoryPath));
        Assert.True(fileSystem.Directory.Exists(job.TargetDirectoryPath));
    }

    [Fact]
    public async Task ExportAsync_ShouldReturnSuccess_WhenNoFilesInSource()
    {
        var job = _fixture.DefaultFileExportJobs.JottacloudTimelineExportJob;
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { job.SourceDirectoryPath, new MockDirectoryData() },
           
        });

        var fileStorage = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);

        var orchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorage);

        var result = await orchestrator.ExportJottacloudTimelineAsync(CancellationToken.None);

        // Assert
        ResultAssert.Success(result);
    }
    #endregion

    #region ExportSteamScreenshots
    [Theory(Skip = "Not refactored")]
    [InlineData(1, "Pung", "P - Papa")]
    [InlineData(12345, "Duum", "D - Delta")]
    [InlineData(987653, "Super Mawio", "S - Sierra")]
    public async Task ExportSteamScreenshotsAsync_ShouldExportSteamScreenshots_ToDirectoryWithAppName(
        uint appId,
        string appName,
        string parentDirectoryName)
    {
        var jobSettings = _fixture.DefaultFileExportJobs.SteamScreenshotsExportJob;

        var mockFileData = new Dictionary<string, MockFileData>();
        var steamRepositoryMock = new Mock<ISteamRepository>();

        var fileByteContent = File.ReadAllBytes(TestDataHelper.Duck);
        string sourceFilePath = Path.Combine(jobSettings.SourceDirectoryPath, appId.ToString(), "image.png");
        mockFileData.Add(sourceFilePath, new MockFileData(fileByteContent));

        steamRepositoryMock
            .Setup(repo => repo.GetAppNameFromId(appId))
            .ReturnsAsync(Result<string>.Success(appName));

        var fileSystemMock = new MockFileSystem(mockFileData);

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorageService,
            steamRepository: steamRepositoryMock.Object);

        var result = await fileExportOrchestrator.ExportSteamScreenshotsAsync();
        var expectedTargetPath = Path.Combine(jobSettings.TargetDirectoryPath, parentDirectoryName, appName, "image.png");

        ResultAssert.Success(result);
        Assert.False(fileSystemMock.File.Exists(sourceFilePath));
        Assert.True(fileSystemMock.File.Exists(expectedTargetPath),
            $"Expected file at path {expectedTargetPath} was not found.");
    }
    #endregion

    #region ExportChromecastPhotos
    [Fact(Skip = "Not refactored")]
    [Trait("API", "Google")]
    public async Task ExportChromecastPhotosAsync_ShouldExportPhots_ToAlbumName()
    {
        //var fileSystem = new FileSystem();
        //var fileStorageService = new FileStorageService(
        //    fileSystem,
        //    new Mock<ILogger<FileStorageService>>().Object);
        //var googlePhotosRepository = new GooglePhotosRepository(
        //    _googlePhotosFixture.MockGooglePhotosLibraryApiCredentials,
        //    new Mock<IGooglePhotosHttpClient>().Object,
        //    new Mock<ILogger<GooglePhotosRepository>>().Object);

        //var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(
        //    fileStorage: fileStorageService,
        //    googlePhotosRepository: googlePhotosRepository);

        //var result = await fileExportOrchestrator.ExportChromecastPhotosAsync();

        //ResultAssert.Success(result);
        // Assert.Equal(2, result.GooglePhotosUploadOperationResults.Count());
    }
    #endregion

    #region ExportDesktopWallpapers
    [Fact(Skip = "Not refactored")]
    public async Task ExportDesktopWallpapersAsync_ShouldExportWallpapers_ToDirectoryBasedOnResolution()
    {
        var imageBytes = await File.ReadAllBytesAsync(TestDataHelper.Egypt);

        var job = _fixture.DefaultFileExportJobs.ScreenshotsExportJob;
        string sourceFilePath = Path.Combine(job.SourceDirectoryPath, Path.GetFileName(TestDataHelper.Egypt));
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
        //var operation = result.Operations.FirstOrDefault();
        ResultAssert.Success(result);
        //Assert.True(result.Operations.Count > 0);
        //Assert.True(operation?.Success == true);
        //Assert.Equal(expectedTargetPath, operation.TargetFilePath);
        Assert.True(fileSystem.File.Exists(expectedTargetPath));
        Assert.False(fileSystem.File.Exists(sourceFilePath));
    }
    #endregion
}