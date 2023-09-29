using PlexLibraryCatalogue;
using PlexLibraryCatalogue.Collectors;
using PlexLibraryCatalogue.Configuration;
using PlexLibraryCatalogue.MediaOrganizers;
using PlexLibraryCatalogue.Uploaders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Construct dependencies 
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Load and register configuration
ApplicationSettingsOptions applicationOptions = new();
builder.Configuration.GetSection(ApplicationSettingsOptions.ApplicationSettings).Bind(applicationOptions);
builder.Services.AddSingleton(applicationOptions);

TautulliOptions tautulliOptions = new();
builder.Configuration.GetSection(TautulliOptions.Tautulli).Bind(tautulliOptions);
builder.Services.AddSingleton(tautulliOptions);

GoogleAPIOptions googleAPIOptions = new();
builder.Configuration.GetSection(GoogleAPIOptions.GoogleAPI).Bind(googleAPIOptions);
builder.Services.AddSingleton(googleAPIOptions);

// Register logging 
builder.Services.AddSerilog(lc => lc
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(Path.Join(builder.Environment.ContentRootPath, "PlexLibraryCatalogue.log"), rollingInterval: RollingInterval.Day));

try
{
    Log.Information($"Logging has been configured. Dumping system configuration:");
    Log.Information(builder.Configuration.GetDebugView());
    Log.Information($"Now registering services and starting application.");

    // Register everything that isn't configuration related
    builder.Services.AddHostedService<CatalogueHostedService>();
    builder.Services.AddSingleton<IDataCollector, TautulliDataCollector>();
    builder.Services.AddSingleton<IUploader, GoogleDriveUploader>();
    builder.Services.AddSingleton<MediaOrganizer>();   

    // Build and run the app
    using IHost host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly!");
}
finally
{
    Log.CloseAndFlush();
}


