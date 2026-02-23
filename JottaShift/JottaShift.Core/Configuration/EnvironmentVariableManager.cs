using JottaShift.Core.FileStorage;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;
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

    public static void InitializeEnvironmentVariables(string filePath, bool deleteFileAfterInit = true)
    {
        var fileSystem = new FileSystem();

        if (!fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found");
        }

        var file = fileSystem.File.ReadAllText(filePath);
        var apiCredentials = JsonSerializer.Deserialize<ApiCredentials>(file) ??
            throw new JsonException("Failed to deserialize API credentials");

        var environmentVariables = MapApiCredentialsToEnvironmentVariables(apiCredentials);

        foreach (var envFromFile in environmentVariables)
        {
            Console.WriteLine(envFromFile.Key);
            var storedEnv = Environment.GetEnvironmentVariable(envFromFile.Key);

            if (envFromFile.Value == storedEnv)
            {
                continue;
            }
            
            Environment.SetEnvironmentVariable(envFromFile.Key, envFromFile.Value);
        }
    }

    private static Dictionary<string, string> MapApiCredentialsToEnvironmentVariables(ApiCredentials apiCredentials)
    {
        var dictonary = new Dictionary<string, string>()
        {
            { _googlePhotosLibraryApiProjectId, apiCredentials.ApiClients.GooglePhotosLibraryApi.Installed.ProjectId },
            { _googlePhotosLibraryApiClientId, apiCredentials.ApiClients.GooglePhotosLibraryApi.Installed.ClientId  },
            { _googlePhotosLibraryApiClientSecret, apiCredentials.ApiClients.GooglePhotosLibraryApi.Installed.ClientSecret },
            { _steamWebApiClientApiKey, apiCredentials.ApiClients.SteamWebApi.ApiKey },
            { _steamWebApiStoreLanguage, apiCredentials.ApiClients.SteamWebApi.StoreLanguage }
        };

        return dictonary;
    }
}
