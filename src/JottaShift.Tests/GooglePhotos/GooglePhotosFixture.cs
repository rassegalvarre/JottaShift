using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.HttpClientWrapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.GooglePhotos;

public class GooglePhotosFixture : IDisposable
{
    public readonly string TestAlbumName = "JottaShift.UnitTests";
    
    public readonly string ValidPhotoDirectoryPath = @"C:\Photos";
    public readonly string ValidPhotoFileName = "photo.jpg";

    public string ValidPhotoFullPath => Path.Combine(ValidPhotoDirectoryPath, ValidPhotoFileName);

    public GooglePhotosLibraryHttpClient CreateGooglePhotosHttpClient(
        IFileStorageService? fileStorage = null,
        IHttpClientWrapper? httpClientWrapper = null,
        IUserCredentialManager? userCredentialManager= null)
    {
        fileStorage ??= new Mock<IFileStorageService>().Object;
        httpClientWrapper ??= new Mock<IHttpClientWrapper>().Object;
        userCredentialManager ??= new Mock<IUserCredentialManager>().Object;

        return new GooglePhotosLibraryHttpClient(
            fileStorage,
            httpClientWrapper,
            userCredentialManager,
            new Mock<ILogger<GooglePhotosLibraryHttpClient>>().Object);
    }

    public GooglePhotosRepository CreateGooglePhotosRepository(
        IGooglePhotosLibraryHttpClient? googlePhotosClient = null)
    {
        googlePhotosClient ??= new Mock<IGooglePhotosLibraryHttpClient>().Object;

        return new GooglePhotosRepository(
            googlePhotosClient,
            new Mock<ILogger<GooglePhotosRepository>>().Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
