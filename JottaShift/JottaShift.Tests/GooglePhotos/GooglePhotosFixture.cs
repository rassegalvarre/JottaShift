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

    public GooglePhotosRepository CreateGooglePhotosRepository(IGooglePhotosClient? googlePhotosClient = null)
    {
        googlePhotosClient ??= new Mock<IGooglePhotosClient>().Object;

        return new GooglePhotosRepository(
            MockGooglePhotosLibraryApiCredentials,
            googlePhotosClient,
            new Mock<ILogger<GooglePhotosRepository>>().Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
