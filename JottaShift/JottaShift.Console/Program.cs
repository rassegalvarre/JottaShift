using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JottaShift.Core.FileStorage;
using JottaShift.Core.TimelineExport;

Console.WriteLine("Hello, World!");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
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
    SourceRoot: @"C:\Photos\Timeline",
    DestinationRoot: @"D:\Exports\Sorted");

Console.WriteLine("Starting timeline export...");

await exporter.ExportAsync(options, new CancellationToken());

Console.WriteLine("Timeline export finished");

await host.StopAsync();