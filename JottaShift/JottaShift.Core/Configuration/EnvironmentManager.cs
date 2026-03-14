using System.IO.Abstractions;
using System.Text.Json;

namespace JottaShift.Core.Configuration;

public class EnvironmentManager()
{
    private static readonly string[] _knownMachineNames = ["BRUTUS", "SKJOLD"];

    private const string _googlePhotosLibraryApiProjectId = "GOOGLEPHOTOSLIBRARAPI_PROJECTID";
    private const string _googlePhotosLibraryApiClientId = "GOOGLEPHOTOSLIBRARAPI_CLIENTID";
    private const string _googlePhotosLibraryApiClientSecret = "GOOGLEPHOTOSLIBRARAPI_CLIENTSECRET";
    private const string _steamWebApiClientApiKey = "STEAMWEBAPI_CLIENTAPIKEY";
    private const string _steamWebApiStoreLanguage = "STEAMWEBAPI_STORELANGUAGE";

    // TODO: Convert to Result<string>
    public static string? GooglePhotosLibraryApiProjectId => Environment.GetEnvironmentVariable(_googlePhotosLibraryApiProjectId);
    public static string? GooglePhotosLibraryApiClientId => Environment.GetEnvironmentVariable(_googlePhotosLibraryApiClientId);
    public static string? GooglePhotosLibraryApiClientSecret => Environment.GetEnvironmentVariable(_googlePhotosLibraryApiClientSecret);
    public static string? SteamWebApiClientApiKey => Environment.GetEnvironmentVariable(_steamWebApiClientApiKey);
    public static string? SteamWebApiStoreLanguage => Environment.GetEnvironmentVariable(_steamWebApiStoreLanguage);

    public static Result<string> GetKnownMachineName()
    {
        string machineName = Environment.MachineName.ToUpper();

        if (_knownMachineNames.Contains(machineName))
        {
            return Result<string>.Success(machineName);
        }
        else
        {
            return Result<string>.Failure("Unkown machine name");
        }
    }
}
