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

    public GooglePhotosHttpClient CreateGooglePhotosHttpClient(
        IFileStorageService? fileStorage = null,
        IHttpClientWrapper? httpClientWrapper = null,
        IUserCredentialManager? userCredentialManager= null)
    {
        fileStorage ??= new Mock<IFileStorageService>().Object;
        httpClientWrapper ??= new Mock<IHttpClientWrapper>().Object;
        userCredentialManager ??= new Mock<IUserCredentialManager>().Object;

        return new GooglePhotosHttpClient(
            fileStorage,
            httpClientWrapper,
            userCredentialManager,
            new Mock<ILogger<GooglePhotosHttpClient>>().Object);
    }

    public GooglePhotosRepository CreateGooglePhotosRepository(
        IGooglePhotosHttpClient? googlePhotosClient = null)
    {
        googlePhotosClient ??= new Mock<IGooglePhotosHttpClient>().Object;

        return new GooglePhotosRepository(
            googlePhotosClient,
            new Mock<ILogger<GooglePhotosRepository>>().Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
