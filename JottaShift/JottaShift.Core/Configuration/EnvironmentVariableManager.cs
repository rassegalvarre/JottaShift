using System.IO.Abstractions;
using System.Text.Json;

namespace JottaShift.Core.Configuration;

public class EnvironmentVariableManager()
{
    private const string _googlePhotosLibraryApiProjectId = "GOOGLEPHOTOSLIBRARAPI_PROJECTID";
    private const string _googlePhotosLibraryApiClientId = "GOOGLEPHOTOSLIBRARAPI_CLIENTID";
    private const string _googlePhotosLibraryApiClientSecret = "GOOGLEPHOTOSLIBRARAPI_CLIENTSECRET";
    private const string _steamWebApiClientApiKey = "STEAMWEBAPI_CLIENTAPIKEY";
    private const string _steamWebApiStoreLanguage = "STEAMWEBAPI_STORELANGUAGE";

    public static string? GooglePhotosLibraryApiProjectId => Environment.GetEnvironmentVariable(_googlePhotosLibraryApiProjectId);
    public static string? GooglePhotosLibraryApiClientId => Environment.GetEnvironmentVariable(_googlePhotosLibraryApiClientId);
    public static string? GooglePhotosLibraryApiClientSecret => Environment.GetEnvironmentVariable(_googlePhotosLibraryApiClientSecret);
    public static string? SteamWebApiClientApiKey => Environment.GetEnvironmentVariable(_steamWebApiClientApiKey);
    public static string? SteamWebApiStoreLanguage => Environment.GetEnvironmentVariable(_steamWebApiStoreLanguage);
}
