using JottaShift.Core.FileExportOrchestrator;
using JottaShift.Core.FileExportOrchestrator.Jobs;
using JottaShift.Core.FileExportOrchestrator.Jobs.FileTransfer;
using JottaShift.Core.FileExportOrchestrator.Jobs.GooglePhotosUpload;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests;

public class FileExportFixture : IDisposable
{
    public readonly string SourceDirectoryRoot = @"C:\source";
    public readonly string TargetDirectoryRoot = @"C:\backup";   
    public Mock<IFileStorage> FileStorageMock => new();
    public Mock<IGooglePhotosRepository> GooglePhotosRepositoryMock => new();
    public Mock<ISteamRepository> SteamRepositoryMock => new();

    public FileExportSettings DefaultFileExportSettings => new()
    {
        FileTransferJobs = [
            new FileTransferJob()
            {
                Key = "desktop_wallpapers",
                SourceDirectoryPath = Path.Combine(SourceDirectoryRoot, "wallpapers"),
                TargetDirectoryPath = Path.Combine(TargetDirectoryRoot, "wallpapers"),
                Enabled = true,
                DeleteSourceFiles = true
            },
            new FileTransferJob()
            {
                Key = "steam_screenshots",
                SourceDirectoryPath = Path.Combine(SourceDirectoryRoot, "steam"),
                TargetDirectoryPath = Path.Combine(TargetDirectoryRoot, "steam"),
                Enabled = true,
                DeleteSourceFiles = true
            },
            new FileTransferJob()
            {
                Key = "jottacloud_timeline",
                SourceDirectoryPath = Path.Combine(SourceDirectoryRoot, "timeline"),
                TargetDirectoryPath = Path.Combine(TargetDirectoryRoot, "timeline"),
                Enabled = true,
                DeleteSourceFiles = true
            }
        ],
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
    };   

    public FileTransferJob DesktopWallpapersJob => DefaultFileExportSettings.FileTransferJobs
        .First(j => j.Key == DefaultJobKeys.DesktopWallpapers);

    public FileTransferJob JottacloudTimelineJob => DefaultFileExportSettings.FileTransferJobs
        .First(j => j.Key == DefaultJobKeys.JottacloudTimeline);

    public FileTransferJob SteamScreenshotsJob => DefaultFileExportSettings.FileTransferJobs
        .First(j => j.Key == DefaultJobKeys.SteamScreenshots);

    public GooglePhotosUploadJob ChromecastUploadJob => DefaultFileExportSettings.GooglePhotosUploadJobs
        .First(j => j.Key == DefaultJobKeys.ChromecastPhotos);

    public FileExportOrchestrator CreateFileExportOrchestrator(
        IFileStorage? fileStorage = null,
        IGooglePhotosRepository? googlePhotosRepository = null,
        ISteamRepository? steamRepository = null
        )
    {
        fileStorage ??= FileStorageMock.Object;
        googlePhotosRepository ??= GooglePhotosRepositoryMock.Object;
        steamRepository ??= SteamRepositoryMock.Object;

        var jobValidator = new FileExportJobValidator(
            new Mock<ILogger<FileExportJobValidator>>().Object,
            DefaultFileExportSettings,
            fileStorage);

        return new FileExportOrchestrator(
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            fileStorage,
            jobValidator,
            googlePhotosRepository, 
            steamRepository);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
