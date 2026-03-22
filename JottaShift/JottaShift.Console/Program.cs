using JottaShift.Core.Configuration;
using JottaShift.Core.FileExport;
using JottaShift.Core.FileExport.Jobs;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.Steam;
using MetadataExtractor.Formats.Photoshop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Linq.Expressions;

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

        services.AddHttpClient<IHttpClientWrapper, HttpClientWrapper>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<IFileSystem, FileSystem>();
        services.AddScoped<IFileWriterFactory, FileWriterFactory>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddSingleton<IGooglePhotosLibraryFacade, GooglePhotosLibraryFacade>();
        services.AddScoped<IGooglePhotosHttpClient, GooglePhotosHttpClient>();
        services.AddScoped<IGooglePhotosRepository, GooglePhotosRepository>();
        services.AddScoped<IUserCredentialManager, UserCredentialManager>();
        services.AddScoped<IJottacloudHttpClient, JottacloudHttpClient>();
        services.AddScoped<IJottacloudRepository, JottacloudRepository>();
        services.AddScoped<ISteamRepository, SteamRepository>();
        services.AddScoped<IFileExportOrchestrator, FileExportOrchestrator>();
        services.AddScoped<IFileExportResultWriter, FileExportResultWriter>();
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

// File transfer jobs
await ExecuteFileTransferJob(
    exportOrchestrator,
    o => o.ExportJottacloudTimelineAsync(new CancellationToken())
);
await ExecuteFileTransferJob(
    exportOrchestrator,
    o => o.ExportSteamScreenshotsAsync(new CancellationToken())
);
await ExecuteFileTransferJob(
    exportOrchestrator,
    o => o.ExportDesktopWallpapersAsync(new CancellationToken())
);

// Album upload jobs
await ExecuteAlbumUploadJob(
    exportOrchestrator,
    o => o.ExportChromecastPhotosAsync(new CancellationToken())
);

await host.StopAsync();

static async Task ExecuteFileTransferJob(
    IFileExportOrchestrator instance,
    Expression<Func<IFileExportOrchestrator, Task<FileTransferJobResult>>> expression)
{
    if (expression.Body is not MethodCallExpression methodCall)
    {
        throw new Exception("Invalid expression call");
    }
    string methodName = methodCall.Method.Name;
    var compiled = expression.Compile();

    Console.WriteLine($"Starting [{methodName}]...");

    var result = await compiled.Invoke(instance);

    Console.WriteLine(Environment.NewLine);
    string resultText = result.Succeeded ? "SUCCEEDED" : "FAILED";
    Console.WriteLine($"Job [{methodName}] {resultText} with status {result.Status}");
    Console.WriteLine($"Files proccessed:       {result.Value?.Count() ?? 0}");
    Console.WriteLine($"Result file location:   {result.ResultFilePath ?? "[Not saved]"}");
    Console.WriteLine($"Error message:          {result.ErrorMessage ?? "[No error]"}");
    Console.WriteLine($"Sources deleted:        {result.SourceDirectoryDeleted}");
    Console.WriteLine(Environment.NewLine);
}

static async Task ExecuteAlbumUploadJob(
    IFileExportOrchestrator instance,
    Expression<Func<IFileExportOrchestrator, Task<AlbumUploadResult>>> expression)
{
    if (expression.Body is not MethodCallExpression methodCall)
    {
        throw new Exception("Invalid expression call");
    }
    string methodName = methodCall.Method.Name;
    var compiled = expression.Compile();

    Console.WriteLine($"Starting [{methodName}]...");

    var result = await compiled.Invoke(instance);

    Console.WriteLine(Environment.NewLine);
    string resultText = result.Succeeded ? "SUCCEEDED" : "FAILED";
    Console.WriteLine($"Job [{methodName}] {resultText}");
    Console.WriteLine($"Album name:             {result.AlbumName}");
    Console.WriteLine($"Files proccessed:       {result.PhotoUploadResults?.Count() ?? 0}");
    Console.WriteLine($"Result file location:   {result.ResultFilePath ?? "[Not saved]"}");
    Console.WriteLine($"Error message:          {result.ErrorMessage ?? "[No error]"}");
    Console.WriteLine(Environment.NewLine);
}