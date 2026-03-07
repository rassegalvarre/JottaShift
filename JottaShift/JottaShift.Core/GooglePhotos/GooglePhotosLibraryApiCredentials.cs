namespace JottaShift.Core.GooglePhotos;

#pragma warning disable IDE1006
public class GooglePhotosLibraryApiCredentials
{
    public required Installed installed { get; init; }
}

public class Installed
{
    public required string client_id { get; set; }
    public required string client_secret { get; set; }
    public required string project_id { get; set; }
    public required string auth_uri { get; init; }
    public required string token_uri { get; init; }
    public required string auth_provider_x509_cert_url { get; init; }
    public required List<string> redirect_uris { get; init; }
}