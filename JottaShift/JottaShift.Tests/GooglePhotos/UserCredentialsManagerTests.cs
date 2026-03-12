namespace JottaShift.Tests.GooglePhotos;

[Trait("Dependency", "Google.Api")]
public class UserCredentialsManagerTests(GooglePhotosFixture _fixture)
    : IClassFixture<GooglePhotosFixture>
{
}
