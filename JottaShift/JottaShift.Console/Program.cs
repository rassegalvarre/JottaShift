using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JottaShift.Core.FileStorage;
using JottaShift.Core.TimelineExport;
using System.IO.Abstractions;

Console.WriteLine("JottaShift initiating..");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IFileStorage, FileStorageService>();
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

var options = new TimelineExportOptions(
    SourceRoot: @"C:\Users\krist\OneDrive\Bilder\JottaShift\Source",
    DestinationRoot: @"C:\Users\krist\OneDrive\Bilder\JottaShift\Target");

Console.WriteLine($"Source directory to copy from:      {options.SourceRoot}");
Console.WriteLine($"Destination directory to copy to:   {options.DestinationRoot}");

Console.WriteLine("Starting timeline export...");

await exporter.ExportAsync(options, new CancellationToken());

Console.WriteLine("Timeline export finished");

await host.StopAsync();