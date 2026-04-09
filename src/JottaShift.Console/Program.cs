using JottaShift.Core;
using JottaShift.Core.Configuration;
using JottaShift.Core.FileExport;
using JottaShift.Core.FileExport.Jobs;
using JottaShift.Core.FileStorage;
using JottaShift.Core.GooglePhotos;
using JottaShift.Core.HttpClientWrapper;
using JottaShift.Core.Jottacloud;
using JottaShift.Core.Steam;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using System.IO.Abstractions;
using System.Linq.Expressions;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/jottashift-.log", rollingInterval: RollingInterval.Minute)
    .CreateLogger();

Console.WriteLine("JottaShift initiating..");

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile(AppSettings.GetAppSettingsFileFullPath(useMachineDefinedSettings: true),
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
        services.AddScoped<IGooglePhotosLibraryHttpClient, GooglePhotosLibraryHttpClient>();
        services.AddScoped<IGooglePhotosRepository, GooglePhotosRepository>();
        services.AddScoped<IUserCredentialManager, UserCredentialManager>();
        services.AddScoped<IJottacloudHttpClient, JottacloudHttpClient>();
        services.AddScoped<IJottacloudRepository, JottacloudRepository>();
        services.AddScoped<ISteamRepository, SteamRepository>();
        services.AddScoped<IFileExportOrchestrator, FileExportOrchestrator>();
        services.AddScoped<IFileExportResultWriter, FileExportResultWriter>();
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

Console.WriteLine("All jobs were executed. Press any key to exit...");
Console.ReadKey();

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

    WriteStart(methodName);

    var result = await compiled.Invoke(instance);

    Console.WriteLine($"    Files proccessed:       {result.Value?.Count() ?? 0}");
    Console.WriteLine($"    Result file location:   {result.ResultFilePath ?? "[Not saved]"}");
    Console.WriteLine($"    Error message:          {result.ErrorMessage ?? "[No error]"}");
    Console.WriteLine($"    Sources deleted:        {result.SourceDirectoryDeleted}");

    WriteFinished(methodName, result);
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

    var separator = new string('═', 60);

    // Start block
    WriteStart(methodName);

    var result = await compiled.Invoke(instance);

    Console.WriteLine($"    Album name:             {result.AlbumName}");
    Console.WriteLine($"    Files processed:        {result.PhotoUploadResults?.Count() ?? 0}");
    Console.WriteLine($"    Result file location:   {result.ResultFilePath ?? "[Not saved]"}");
    Console.WriteLine($"    Error message:          {result.ErrorMessage ?? "[No error]"}");

    // Status line
    WriteFinished(methodName, result);
   
}

static string Separator() => new string('═', 60);

static void WriteStart(string methodName)
{
    var startTimestamp = DateTime.Now.ToString("HH:mm:ss");

    Console.WriteLine();
    Console.WriteLine(Separator());
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"[{startTimestamp}] STARTING: {methodName}");
    Console.ResetColor();
    Console.WriteLine(Separator());
}

static void WriteFinished(string methodName, Result result)
{
    var endTimestamp = DateTime.Now.ToString("HH:mm:ss");
    Console.WriteLine(Separator());
    if (result.Succeeded)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{endTimestamp}] JOB [{methodName}] SUCCEEDED");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{endTimestamp}] JOB [{methodName}] FAILED");
    }
    Console.ResetColor();
    Console.WriteLine();
}