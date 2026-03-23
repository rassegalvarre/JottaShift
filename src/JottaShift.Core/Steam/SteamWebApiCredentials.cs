namespace JottaShift.Core.Steam;

#pragma warning disable IDE1006
public class SteamWebApiCredentials
{
    public required string api_key { get; init; }
    public required string domain_name { get; init; }
    public required string store_language { get; init; }
}