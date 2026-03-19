using JottaShift.Core.Configuration;
using JottaShift.Core.FileExport;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.Steam;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

Console.WriteLine("JottaShift initiating..");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile(AppSettings.GetAppSettingsFileFullPath(),
            optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<AppSettings>(
            hostContext.Configuration);

        services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<AppSettings>>().Value.FileExportJobs);

        services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<AppSettings>>().Value.GooglePhotosLibraryApiCredentials);

        services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<AppSettings>>().Value.JottacloudSettings);

        services.AddSingleton(resolver =>
           resolver.GetRequiredService<IOptions<AppSettings>>().Value.SteamWebApiCredentials);

        services.AddHttpClient<IHttpClientWrapper, JottaShift.Core.HttpClientWrapper.HttpClientWrapper>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddSingleton<IGooglePhotosLibraryFacade, GooglePhotosLibraryFacade>();
        services.AddScoped<IGooglePhotosHttpClient, GooglePhotosHttpClient>();
        services.AddScoped<IGooglePhotosRepository, GooglePhotosRepository>();
        services.AddScoped<IUserCredentialManager, UserCredentialManager>();
        services.AddScoped<IJottacloudHttpClient, JottacloudHttpClient>();
        services.AddScoped<IJottacloudRepository, JottacloudRepository>();
        services.AddScoped<ISteamRepository, SteamRepository>();
        services.AddScoped<IFileExportOrchestrator, FileExportOrchestrator>();
    })
    // TODO: In Release config, save logs to file.
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

await host.StartAsync();

using var scope = host.Services.CreateScope();
var exportOrchestrator = scope.ServiceProvider.GetRequiredService<IFileExportOrchestrator>();

Console.WriteLine("Starting timeline export...");
var timelineExportResult = await exportOrchestrator.ExportJottacloudTimelineAsync(new CancellationToken());
Console.WriteLine("Timeline export finished with result {0}", timelineExportResult.Succeeded);

Console.WriteLine("Starting Chromecast upload...");
var chromecastUploadResult = await exportOrchestrator.ExportChromecastPhotosAsync(new CancellationToken());
Console.WriteLine("Chromecast upload finished with result {0}", chromecastUploadResult.Succeeded);

Console.WriteLine("Starting Steam screenshort export...");
var steamExportResult = await exportOrchestrator.ExportSteamScreenshotsAsync(new CancellationToken());
Console.WriteLine("Steam screenshot export finished with result {0}", steamExportResult.Succeeded);

Console.WriteLine("Starting wallpaper export...");
var wallpaperExportResult = await exportOrchestrator.ExportDesktopWallpapersAsync(new CancellationToken());
Console.WriteLine("Wallpaper export finished with result {0}", wallpaperExportResult.Succeeded);

await host.StopAsync();