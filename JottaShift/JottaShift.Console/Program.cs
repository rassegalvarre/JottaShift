using JottaShift.Core.Configuration;
using JottaShift.Core.FileExportOrchestrator;
using JottaShift.Core.FileExportOrchestrator.Jobs;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
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
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<AppSettings>(
            hostContext.Configuration);

        services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<AppSettings>>().Value.FileExportSettings);

        services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<AppSettings>>().Value.GooglePhotosLibraryApiCredentials);

        services.AddSingleton(resolver =>
           resolver.GetRequiredService<IOptions<AppSettings>>().Value.SteamWebApiCredentials);


        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IFileStorage, FileStorageService>();
        services.AddScoped<IGooglePhotosRepository, GooglePhotosRepository>();
        services.AddScoped<ISteamRepository, SteamRepository>();
        services.AddScoped<IFileExportJobValidator, FileExportJobValidator>();
        services.AddScoped<IFileExportOrchestrator, FileExportOrchestrator>();
    })
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
var exportSettings = scope.ServiceProvider.GetRequiredService<FileExportSettings>();
var creds = scope.ServiceProvider.GetRequiredService<GooglePhotosLibraryApiCredentials>();
var credsSteam = scope.ServiceProvider.GetRequiredService<SteamWebApiCredentials>();
var exportOrchestrator = scope.ServiceProvider.GetRequiredService<IFileExportOrchestrator>();

//var config = new ConfigurationBuilder()
//    .SetBasePath(Directory.GetCurrentDirectory()) 
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .Build();

Console.WriteLine("Starting timeline export...");
var timelineExportResult = await exportOrchestrator.ExportJottacloudTimelineAsync(new CancellationToken());
Console.WriteLine("Timeline export finished with result {0}", timelineExportResult.Status);

Console.WriteLine("Starting Chromecast upload...");
var chromecastUploadResult = await exportOrchestrator.ExportChromecastPhotosAsync(new CancellationToken());
Console.WriteLine("Chromecast upload finished with result {0}", chromecastUploadResult.Status);

Console.WriteLine("Starting Steam screenshort export...");
var steamExportResult = await exportOrchestrator.ExportSteamScreenshotsAsync(new CancellationToken());
Console.WriteLine("Steam screenshot export finished with result {0}", steamExportResult.Status);

Console.WriteLine("Starting wallpaper export...");
var wallpaperExportResult = await exportOrchestrator.ExportDesktopWallpapersAsync(new CancellationToken());
Console.WriteLine("Wallpaper export finished with result {0}", wallpaperExportResult.Status);


await host.StopAsync();