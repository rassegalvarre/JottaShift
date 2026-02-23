using System.Collections.Generic;
using System.Text.Json.Serialization;

// TODO: Move to EnvironmentVariables or similar, to avoid storing secrets in json
public class ApiCredentials
{
    [JsonPropertyName("api_clients")]
    public required ApiClients ApiClients { get; init; }
}

public class ApiClients
{
    [JsonPropertyName("google_photos_library_api")]
    public required GooglePhotosLibraryApi GooglePhotosLibraryApi { get; init; }

    [JsonPropertyName("steam_web_api")]
    public required SteamWebApi SteamWebApi { get; init; }
}

public class GooglePhotosLibraryApi
{
    [JsonPropertyName("installed")]
    public required Installed Installed { get; init; }
}

public class Installed
{
    [JsonPropertyName("client_id")]
    public required string ClientId { get; init; }

    [JsonPropertyName("client_secret")]
    public required string ClientSecret { get; init; }

    [JsonPropertyName("project_id")]
    public required string ProjectId { get; init; }

    [JsonPropertyName("auth_uri")]
    public required string AuthUri { get; init; }

    [JsonPropertyName("token_uri")]
    public required string TokenUri { get; init; }

    [JsonPropertyName("auth_provider_x509_cert_url")]
    public required string AuthProviderX509CertUrl { get; init; }

    [JsonPropertyName("redirect_uris")]
    public required List<string> RedirectUris { get; init; }
}

public class SteamWebApi
{
    [JsonPropertyName("api_key")]
    public required string ApiKey { get; init; }

    [JsonPropertyName("domain_name")]
    public required string DomainName { get; init; }

    [JsonPropertyName("store_language")]
    public required string StoreLanguage { get; init; }
}