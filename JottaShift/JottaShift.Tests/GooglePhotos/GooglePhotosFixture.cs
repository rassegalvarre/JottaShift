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

    public GooglePhotosLibraryApiCredentials MockGooglePhotosLibraryApiCredentials = new()
    {
        installed = new()
        {
            auth_provider_x509_cert_url = string.Empty,
            auth_uri = string.Empty,
            client_id = string.Empty,
            client_secret = string.Empty,
            project_id = string.Empty,
            redirect_uris = [],
            token_uri = string.Empty
        }
    };

    public GooglePhotosHttpClient CreateGooglePhotosHttpClient(
        IFileStorage? fileStorage = null,
        IHttpClientWrapper? httpClientWrapper = null,
        IUserCredentialManager? userCredentialManager= null)
    {
        fileStorage ??= new Mock<IFileStorage>().Object;
        httpClientWrapper ??= new Mock<IHttpClientWrapper>().Object;
        userCredentialManager ??= new Mock<IUserCredentialManager>().Object;

        return new GooglePhotosHttpClient(
            fileStorage,
            httpClientWrapper,
            userCredentialManager,
            new Mock<ILogger<GooglePhotosHttpClient>>().Object);
    }

    public GooglePhotosRepository CreateGooglePhotosRepository(
        IGooglePhotosLibraryFacade? googlePhotosLibraryFacade= null,
        IGooglePhotosHttpClient? googlePhotosClient = null)
    {
        googlePhotosLibraryFacade ??= new Mock<IGooglePhotosLibraryFacade>().Object;
        googlePhotosClient ??= new Mock<IGooglePhotosHttpClient>().Object;

        return new GooglePhotosRepository(
            googlePhotosLibraryFacade,
            googlePhotosClient,
            new Mock<ILogger<GooglePhotosRepository>>().Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
