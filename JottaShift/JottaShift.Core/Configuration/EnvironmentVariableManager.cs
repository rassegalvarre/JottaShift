using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Core.Configuration;

public class EnvironmentVariableManager
{
    public static string? GooglePhotosLibraryApiProjectId => Environment.GetEnvironmentVariable("GooglePhotosLibraryApi_ProjectId");
    public static string? GooglePhotosLibraryApiClientId => Environment.GetEnvironmentVariable("GooglePhotosLibraryApi_ClientId");
    public static string? GooglePhotosLibraryApiClientSecret => Environment.GetEnvironmentVariable("GooglePhotosLibraryApi_ClientSecret");
    public static string? SteamWebApiClientApiKey = Environment.GetEnvironmentVariable("SteamWebApi_ClientApiKey");
    public static string? SteamWebApiStoreLanguage = Environment.GetEnvironmentVariable("SteamWebApi_StoreLanguage");

    private static readonly string?[] _environmentVariableNames =
    [
        GooglePhotosLibraryApiProjectId,
        GooglePhotosLibraryApiClientId,
        GooglePhotosLibraryApiClientSecret,
        SteamWebApiClientApiKey,
        SteamWebApiStoreLanguage,
    ];

    // Prompt each variable from the user, and set it in the environment 
    public static void InitializeEnvironmentVariables()
    {
        foreach (var environmentVariable in _environmentVariableNames)
        {
            Console.WriteLine(environmentVariable);
            //if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(environmentVariable)))
            //{
            //    continue; // Skip if already set
            //}

            //Console.Write($"Enter value for {environmentVariable}: ");
            //var value = Console.ReadLine();
            //if (string.IsNullOrEmpty(value))
            //{
            //    Console.WriteLine($"Value for {environmentVariable} cannot be empty. Please try again.");
            //    return;
            //}
            //Environment.SetEnvironmentVariable(environmentVariable, value);
        }
    }
}
