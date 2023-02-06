using System.Globalization;
using Hellang.Middleware.ProblemDetails.Mvc;
using Heroplate.Api.Application;
using Heroplate.Api.Host.Configurations;
using Heroplate.Api.Host.Controllers;
using Heroplate.Api.Infrastructure;
using Heroplate.Api.Infrastructure.AppSettings;
using Heroplate.Api.Infrastructure.Common.Logging;
using Serilog;

[assembly: ApiConventionType(typeof(BaseApiConventions))]

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

    builder.Configuration.AddConfigurations(builder.Environment.EnvironmentName);
    builder.AddAppSettingsConfiguration();

    builder.Host.UseSerilog((_, config) => config
        .WriteTo.Console(formatProvider: CultureInfo.CurrentCulture)
        .ReadFrom.Configuration(builder.Configuration));

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddApplication();

    builder.Services
        .AddControllers(options => options
                .Filters.Add<SerilogLoggingActionFilter>()) // Adds some action properties and the tenant to the log context
            .AddProblemDetailsConventions(); // Adds MVC conventions to work better with the ProblemDetails middleware.

    var app = builder.Build();

    await app.Services.InitializeDatabasesAsync();

    app.UseInfrastructure(builder.Configuration);

    app.MapEndpoints();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    await Log.CloseAndFlushAsync();
}