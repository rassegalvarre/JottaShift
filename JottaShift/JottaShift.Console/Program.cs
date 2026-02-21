using JottaShift.Core.FileStorage;
using JottaShift.Core.TimelineExport;
using JottaShift.Core.GooglePhotos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

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
        services.AddScoped<IGooglePhotos, GooglePhotosRepository>();
        services.AddScoped<ITimelineExport, TimelineExportService>();
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
var exporter = scope.ServiceProvider.GetRequiredService<ITimelineExport>();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory()) 
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var options = new TimelineExportOptions(
    SourceRoot: config.GetValue<string>("SourceRoot")!,
    DestinationRoot: config.GetValue<string>("DestinationRoot")!);

Console.WriteLine($"Source directory to copy from:      {options.SourceRoot}");
Console.WriteLine($"Destination directory to copy to:   {options.DestinationRoot}");

Console.WriteLine("Starting timeline export...");

await exporter.ExportAsync(options, new CancellationToken());

Console.WriteLine("Timeline export finished");

await host.StopAsync();