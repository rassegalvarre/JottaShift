using JottaShift.Core;
using JottaShift.Core.FileExport;
using JottaShift.Core.FileExport.Jobs;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.Jottacloud.Models.Dto;
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

        var alphabeticParentDirectoryName = FileExportOrchestrator.GetAlphabeticParentDirectoryName(directoryName);

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
        FileTransferResultAssert.SuccessfullJob(result);

        Assert.False(fileSystem.File.Exists(sourceFilePath));
        Assert.True(fileSystem.File.Exists(expectedFilePath));
        Assert.Empty(fileSystem.Directory.EnumerateFileSystemEntries(
            _fixture.DefaultFileExportJobs.JottacloudTimelineExportJob.SourceDirectoryPath));
    }

    [Fact]
    public async Task ExportAsync_ShouldNotDeleteSourceDirectoryContent_WhenOneCopyFails()
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
        FileTransferResultAssert.FailedJob(result);
        FileTransferResultAssert.FailedTransfer(result.Value!.Last());

        Assert.NotEmpty(fileSystem.Directory.EnumerateFileSystemEntries(
            _fixture.DefaultFileExportJobs.JottacloudTimelineExportJob.SourceDirectoryPath));
    }

    [Fact]
    public async Task ExportAsync_ShouldReturnSuccessAndCreateDirectories_WhenNoneExists()
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
        FileTransferResultAssert.SuccessfullJob(result);
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
        FileTransferResultAssert.SuccessfullJob(result);
    }
    #endregion

    #region ExportSteamScreenshots
    [Theory]
    [InlineData(1, "Pung", "P - Papa")]
    [InlineData(12345, "Duum", "D - Delta")]
    [InlineData(987653, "Super Mawio", "S - Sierra")]
    public async Task ExportSteamScreenshotsAsync_ShouldExportSingleSteamScreenshot_ToDirectoryWithAppName(
        uint appId,
        string appName,
        string parentDirectoryName)
    {
        var jobSettings = _fixture.DefaultFileExportJobs.SteamScreenshotsExportJob;

        string imageFileName = "some-image.png";
        string sourceFilePath = Path.Combine(jobSettings.SourceDirectoryPath, appId.ToString(), imageFileName);
        var expectedTargetPath = Path.Combine(jobSettings.TargetDirectoryPath, parentDirectoryName, appName, imageFileName);

        var steamRepositoryMock = new Mock<ISteamRepository>();
        steamRepositoryMock
            .Setup(repo => repo.GetAppNameFromId(appId))
            .ReturnsAsync(Result<string>.Success(appName));

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { sourceFilePath, new MockFileData([]) }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorageService,
            steamRepository: steamRepositoryMock.Object);

        var result = await fileExportOrchestrator.ExportSteamScreenshotsAsync();

        FileTransferResultAssert.SuccessfullJob(result);
        Assert.False(fileSystemMock.File.Exists(sourceFilePath));
        Assert.True(fileSystemMock.File.Exists(expectedTargetPath),
            $"Expected file at path {expectedTargetPath} was not found.");
    }

    [Fact]
    public async Task ExportSteamScreenshotsAsync_ShouldSkipDirectory_WhoseNameIsInvalidSteamAppId()
    {
        var jobSettings = _fixture.DefaultFileExportJobs.SteamScreenshotsExportJob;

        string sourceFilePath = Path.Combine(jobSettings.SourceDirectoryPath, "invalid-app-id", "image.png");

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { sourceFilePath, new MockFileData([]) }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorageService);

        var result = await fileExportOrchestrator.ExportSteamScreenshotsAsync();

        FileTransferResultAssert.SuccessfullJob(result);
    }

    [Fact]
    public async Task ExportSteamScreenshotsAsync_ShouldSkipDirectory_WhoseAppNameIsNotFoundd()
    {
        var jobSettings = _fixture.DefaultFileExportJobs.SteamScreenshotsExportJob;

        string sourceFilePath = Path.Combine(jobSettings.SourceDirectoryPath, "98765", "image.png");

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { sourceFilePath, new MockFileData([]) }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var steamRepositoryMock = new Mock<ISteamRepository>();
        steamRepositoryMock
            .Setup(repo => repo.GetAppNameFromId(It.IsAny<uint>()))
            .ReturnsAsync(Result<string>.Failure("Invalid app id"));

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorageService,
            steamRepository: steamRepositoryMock.Object);

        var result = await fileExportOrchestrator.ExportSteamScreenshotsAsync();

        FileTransferResultAssert.SuccessfullJob(result);
    }

    [Fact]
    public async Task ExportSteamScreenshotsAsync_ShouldDeleteThumbnails()
    {
        var jobSettings = _fixture.DefaultFileExportJobs.SteamScreenshotsExportJob;

        string sourceFilePath = Path.Combine(
            jobSettings.SourceDirectoryPath,
            "98765",
            "thumbnails",
            "image.png");

        var fileSystemMock = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { sourceFilePath, new MockFileData([]) }
        });

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var steamRepositoryMock = new Mock<ISteamRepository>();
        steamRepositoryMock
            .Setup(repo => repo.GetAppNameFromId(It.IsAny<uint>()))
            .ReturnsAsync(Result<string>.Success("Half Knife 2"));

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorageService,
            steamRepository: steamRepositoryMock.Object);

        var result = await fileExportOrchestrator.ExportSteamScreenshotsAsync();
        var transfer = result.Value!.First()!;

        FileTransferResultAssert.SuccessfullJob(result);
        Assert.False(fileSystemMock.File.Exists(sourceFilePath));
    }

    [Fact]
    public async Task ExportSteamScreenshotsAsync_ShouldTransferMultipleApps_AndHandleMisplacedDirectory()
    {
        var jobSettings = _fixture.DefaultFileExportJobs.SteamScreenshotsExportJob;
        string basePath = jobSettings.SourceDirectoryPath;

        var screenshotDirectory = new Dictionary<string, MockFileData>
        {
            // Half Knife 2
            { Path.Combine(basePath, "98765", "screenshot_1.jpg"), new MockFileData([]) },
            { Path.Combine(basePath, "98765", "screenshot_2.jpg"), new MockFileData([]) },
            { Path.Combine(basePath, "98765", "thumbnails", "thumb.jpg"), new MockFileData([]) },

            // Duum
            { Path.Combine(basePath, "12345", "screenshot_1.jpg"), new MockFileData([]) },
            { Path.Combine(basePath, "12345", "screenshot_2.jpg"), new MockFileData([]) },
            { Path.Combine(basePath, "12345", "thumbnails", "thumb.jpg"), new MockFileData([]) },

            // Misplaced directory that should be ignored
            { Path.Combine(basePath, "temp", "temp.bin"), new MockFileData([]) },
        };

        var fileSystemMock = new MockFileSystem(screenshotDirectory);

        var fileStorageService = new FileStorageService(
            fileSystemMock,
            new Mock<ILogger<FileStorageService>>().Object);

        var steamRepositoryMock = new Mock<ISteamRepository>();
        steamRepositoryMock
            .Setup(repo => repo.GetAppNameFromId(98765))
            .ReturnsAsync(Result<string>.Success("Half Knife 2"));
        steamRepositoryMock
            .Setup(repo => repo.GetAppNameFromId(12345))
            .ReturnsAsync(Result<string>.Success("Duum"));

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(
            fileStorage: fileStorageService,
            steamRepository: steamRepositoryMock.Object);

        var result = await fileExportOrchestrator.ExportSteamScreenshotsAsync();

        FileTransferResultAssert.SuccessfullJob(result);

        foreach(var file in screenshotDirectory)
        {
            if (file.Key.Contains("temp")) // Should not be included in result, but deleted from source
            {
                Assert.DoesNotContain(result.Value!, 
                    v => v.SourceFileFullPath == file.Key);

                Assert.False(fileSystemMock.File.Exists(file.Key),
                    $"Misplaced file at path {file.Key} was expected to be ignored but was not found.");
                continue;
            }

            var transferResult = result.Value!.First(r => r.SourceFileFullPath == file.Key);

            // Transfer has been asserted at this stage. 
            // Always delete source file in Steam-folder
            Assert.False(fileSystemMock.File.Exists(transferResult.SourceFileFullPath)); 

            if (file.Key.Contains("thumbnails")) // Thumbnails should be deleted and not transferred
            {
                Assert.False(fileSystemMock.File.Exists(transferResult.NewFileFullPath),
                    $"Thumbnail file at path {file.Key} was expected to be deleted but was found.");
                continue;
            }
            else
            {
                Assert.True(fileSystemMock.File.Exists(transferResult.NewFileFullPath));
            }
        }
    }
    #endregion

    #region ExportChromecastPhotos
    private PhotoDto GetPhotoDto(bool hasLocalPath)
    {
        string imageName = Path.GetRandomFileName();
        return new PhotoDto(Guid.NewGuid().ToString(), imageName, new DateTimeOffset()) with
        {
            LocalFilePath = hasLocalPath ? Path.Combine(_fixture.SourceDirectoryRoot, imageName) : null
        };
    }

    [Fact]
    public async Task ExportChromecastPhotosAsync_ReturnFailure_WhenIncompleteUpload()
    {
        var job = _fixture.DefaultFileExportJobs.ChromecastUploadJob;
        
        var album = new AlbumDto()
        {
            AlbumTitle = "Staging album title",
            Photos = [
                GetPhotoDto(true),
                GetPhotoDto(true),
                GetPhotoDto(false)
            ]
        };

        var photoUploadResults = album.Photos.Select(p => new PhotoUploadResult(p.LocalFilePath!));
        var albumUploadResult = AlbumUploadResult.Failure(album.AlbumTitle, "Incomplete upload", photoUploadResults);

        var jottacloudRepositoryMock = new Mock<IJottacloudRepository>();
        jottacloudRepositoryMock.Setup(r => r.GetAlbumAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<AlbumDto>.Success(album));

        var googlePhotosRepositoryMock = new Mock<IGooglePhotosRepository>();
        googlePhotosRepositoryMock.Setup(r => r.UploadPhotosToAlbumAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(albumUploadResult);

        var orchestrator = _fixture.CreateFileExportOrchestrator(
            googlePhotosRepository: googlePhotosRepositoryMock.Object,
            jottacoudRepository: jottacloudRepositoryMock.Object);

        var result = await orchestrator.ExportChromecastPhotosAsync();

        ResultAssert.Failure(result);
    }
    #endregion

    #region ExportDesktopWallpapers
    [Fact]
    [Trait("Dependency", "FileSystem")]
    public async Task ExportDesktopWallpapersAsync_ShouldExportImageWithKnownResolution_ToDirectoryBasedOnResolution()
    {
        var job = _fixture.DefaultFileExportJobs.ScreenshotsExportJob;

        string sourceImageFileName = Path.GetFileName(TestDataHelper.Egypt);
        var sourceImageContentBytes = await File.ReadAllBytesAsync(TestDataHelper.Egypt);

        string sourceFilePath = Path.Combine(job.SourceDirectoryPath, sourceImageFileName);
        string expectedTargetPath = Path.Combine(job.TargetDirectoryPath, "4K", sourceImageFileName);

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { sourceFilePath, new MockFileData(sourceImageContentBytes) },
        });

        var fileStorageService = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(fileStorage: fileStorageService);

        var result = await fileExportOrchestrator.ExportDesktopWallpapersAsync();

        FileTransferResultAssert.SuccessfullJob(result);

        Assert.False(fileSystem.File.Exists(sourceFilePath));
        Assert.True(fileSystem.File.Exists(expectedTargetPath));
    }

    [Fact]
    [Trait("Dependency", "FileSystem")]
    public async Task ExportDesktopWallpapersAsync_ShouldIgnoreImageWithUnknownResolution()
    {
        var job = _fixture.DefaultFileExportJobs.ScreenshotsExportJob;

        string sourceImageFileName = Path.GetFileName(TestDataHelper.Duck);
        var sourceImageContentBytes = await File.ReadAllBytesAsync(TestDataHelper.Duck);

        string sourceFilePath = Path.Combine(job.SourceDirectoryPath, sourceImageFileName);

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            { sourceFilePath, new MockFileData(sourceImageContentBytes) },
        });

        var fileStorageService = new FileStorageService(
            fileSystem,
            new Mock<ILogger<FileStorageService>>().Object);

        var fileExportOrchestrator = _fixture.CreateFileExportOrchestrator(fileStorage: fileStorageService);

        var result = await fileExportOrchestrator.ExportDesktopWallpapersAsync();

        FileTransferResultAssert.FailedJob(result);
        Assert.Single(result.Value!);
        Assert.Equal(FileTransferResultStatus.InvalidSourceFile, result.Value!.First().Status);
        Assert.True(fileSystem.File.Exists(sourceFilePath));
    }
    #endregion
}