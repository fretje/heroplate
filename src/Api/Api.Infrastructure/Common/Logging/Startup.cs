using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace Heroplate.Api.Infrastructure.Common.Logging;

internal static class Startup
{
    internal static IServiceCollection AddRequestLogging(this IServiceCollection services, IConfiguration config) =>
        services.Configure<LoggerSettings>(config.GetSection(nameof(LoggerSettings)));

    internal static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetRequiredService<IOptions<LoggerSettings>>().Value.EnableRequestLogging)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = LogHelper.EnrichFromRequest;
                options.IncludeQueryInRequestPath = true;
                options.GetLevel = LogHelper.GetLevel(
                    LogEventLevel.Debug,
                    "/notifications/negotiate",
                    "/notifications");
            });
        }

        return app;
    }
}