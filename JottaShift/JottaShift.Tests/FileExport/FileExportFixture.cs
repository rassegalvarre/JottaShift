using JottaShift.Core.FileExport;
using JottaShift.Core.FileExport.Jobs;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.Steam;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.FileExport;

public class FileExportFixture : IDisposable
{
    public readonly string SourceDirectoryRoot = @"C:\source";
    public readonly string TargetDirectoryRoot = @"C:\backup";   
    public Mock<IFileStorageService> FileStorageMock => new();
    public Mock<IGooglePhotosRepository> GooglePhotosRepositoryMock => new();
    public Mock<IJottacloudRepository> JottacloudRepositoryMock => new();

    public Mock<ISteamRepository> SteamRepositoryMock => new();

    public FileExportJobs DefaultFileExportJobs => new()
    {
        ScreenshotsExportJob = new FileTransferJob()
        {
            Id = "desktop_wallpapers",
            SourceDirectoryPath = Path.Combine(SourceDirectoryRoot, "wallpapers"),
            TargetDirectoryPath = Path.Combine(TargetDirectoryRoot, "wallpapers"),
            Enabled = true,
            DeleteSourceFiles = true
        },
        SteamScreenshotsExportJob = new FileTransferJob()
        {
            Id = "steam_screenshots",
            SourceDirectoryPath = Path.Combine(SourceDirectoryRoot, "steam"),
            TargetDirectoryPath = Path.Combine(TargetDirectoryRoot, "steam"),
            Enabled = true,
            DeleteSourceFiles = true
        },
        JottacloudTimelineExportJob = new FileTransferJob()
        {
            Id = "jottacloud_timeline",
            SourceDirectoryPath = Path.Combine(SourceDirectoryRoot, "timeline"),
            TargetDirectoryPath = Path.Combine(TargetDirectoryRoot, "timeline"),
            Enabled = true,
            DeleteSourceFiles = true
        },
        ChromecastUploadJob = new ChromecastUploadJob()
        {
            Id = "chromecast_photos",
            SourceJottacloudAlbumSharedUrl = "https://www.jottacloud.com/share/imjg7a52t61g",
            TargetGooglePhotosAlbumName = "JottaSync.UnitTests.FileExport",
            Enabled = true
        }        
    };

    public FileExportOrchestrator CreateFileExportOrchestrator(
        IFileStorageService? fileStorage = null,
        IGooglePhotosRepository? googlePhotosRepository = null,
        IJottacloudRepository? jottacoudRepository = null,
        ISteamRepository? steamRepository = null
        )
    {
        fileStorage ??= FileStorageMock.Object;
        googlePhotosRepository ??= GooglePhotosRepositoryMock.Object;
        jottacoudRepository ??= JottacloudRepositoryMock.Object;
        steamRepository ??= SteamRepositoryMock.Object;

        return new FileExportOrchestrator(
            DefaultFileExportJobs,
            new Mock<ILogger<FileExportOrchestrator>>().Object,
            fileStorage,
            googlePhotosRepository, 
            jottacoudRepository,
            steamRepository);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
