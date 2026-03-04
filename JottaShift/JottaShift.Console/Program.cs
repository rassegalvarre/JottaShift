using JottaShift.Core.FileStorage;
using JottaShift.Core.FileExportOrchestrator;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.SteamRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using JottaShift.Core.Configuration;
using JottaShift.Core.FileExportOrchestrator.Jobs;

Console.WriteLine("JottaShift initiating..");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
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

// TODO: Improve
EnvironmentVariableManager.InitializeEnvironmentVariables(@"C:\<path>\api_credentials.json");

using var scope = host.Services.CreateScope();
var exportOrchestrator = scope.ServiceProvider.GetRequiredService<IFileExportOrchestrator>();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory()) 
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

Console.WriteLine("Starting timeline export...");
var timelineExportResult = await exportOrchestrator.ExportJottacloudTimelineAsync(new CancellationToken());
Console.WriteLine("Timeline export finished with result {Status}", timelineExportResult.Status);

Console.WriteLine("Starting Chromecast upload...");
var chromecastUploadResult = await exportOrchestrator.ExportChromecastPhotosAsync(new CancellationToken());
Console.WriteLine("Chromecast upload finished with result {Status}", chromecastUploadResult.Status);

Console.WriteLine("Starting Steam screenshort export...");
var steamExportResult = await exportOrchestrator.ExportSteamScreenshotsAsync(new CancellationToken());
Console.WriteLine("Steam screenshot export finished with result {Status}", steamExportResult.Status);

Console.WriteLine("Starting wallpaper export...");
var wallpaperExportResult = await exportOrchestrator.ExportDesktopWallpapersAsync(new CancellationToken());
Console.WriteLine("Wallpaper export finished with result {Status}", wallpaperExportResult.Status);


await host.StopAsync();