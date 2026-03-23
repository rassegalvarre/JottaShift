namespace JottaShift.Core.Configuration;

public class EnvironmentManager()
{
    private static readonly string[] _knownMachineNames = ["BRUTUS", "SKJOLD"];

    public const string GooglePhotosLibraryApiProjectId = "GOOGLEPHOTOSLIBRARAPI_PROJECTID";
    public const string GooglePhotosLibraryApiClientId = "GOOGLEPHOTOSLIBRARAPI_CLIENTID";
    public const string GooglePhotosLibraryApiClientSecret = "GOOGLEPHOTOSLIBRARAPI_CLIENTSECRET";
    public const string SteamWebApiClientApiKey = "STEAMWEBAPI_CLIENTAPIKEY";
    public const string SteamWebApiStoreLanguage = "STEAMWEBAPI_STORELANGUAGE";

    public static bool TryGetEnvironmentVariable(string key, out string value)
    {
        var foundValue = Environment.GetEnvironmentVariable(key);
        if (foundValue == null)
        {
            value = string.Empty;
            return false;
        }

        value = foundValue;
        return true;
    }

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
