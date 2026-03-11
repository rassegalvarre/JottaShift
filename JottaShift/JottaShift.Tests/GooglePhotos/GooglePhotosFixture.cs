using JottaShift.Core.GooglePhotos;
using Microsoft.Extensions.Logging;
using Moq;

namespace JottaShift.Tests.GooglePhotos;

public class GooglePhotosFixture : IDisposable
{
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
