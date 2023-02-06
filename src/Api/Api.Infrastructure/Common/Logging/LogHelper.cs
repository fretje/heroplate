using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;

namespace Heroplate.Api.Infrastructure.Common.Logging;

internal static class LogHelper
{
    public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        var request = httpContext.Request;

        // Set all the common properties available for every request
        diagnosticContext.Set("Host", request.Host);
        diagnosticContext.Set("Protocol", request.Protocol);
        diagnosticContext.Set("Scheme", request.Scheme);

        // Set the content-type of the Response at this point
        diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

        // Retrieve the IEndpointFeature selected for the request
        var endpoint = httpContext.GetEndpoint();
        if (endpoint is not null)
        {
            diagnosticContext.Set("EndpointName", endpoint.DisplayName);
        }

        if (httpContext.User.Identity?.IsAuthenticated is true)
        {
            diagnosticContext.Set("User", httpContext.User.Identity.Name);
        }

        diagnosticContext.Set("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), true);
    }

    /// <summary>
    /// Create a <see cref="Serilog.AspNetCore.RequestLoggingOptions.GetLevel" /> method that
    /// uses the default logging level, except for the specified endpoint names, which are
    /// logged using the provided <paramref name="traceLevel" />.
    /// </summary>
    /// <param name="traceLevel">The level to use for logging "trace" endpoints.</param>
    /// <param name="traceEndpointNames">The display name of endpoints to be considered "trace" endpoints.</param>
    public static Func<HttpContext, double, Exception?, LogEventLevel> GetLevel(LogEventLevel traceLevel, params string[] traceEndpointNames) =>
        traceEndpointNames is null || traceEndpointNames.Length == 0
            ? throw new ArgumentNullException(nameof(traceEndpointNames))
            : ((ctx, _, ex) => IsError(ctx, ex)
                ? LogEventLevel.Error
                : IsTraceEndpoint(ctx, traceEndpointNames)
                    ? traceLevel
                    : LogEventLevel.Information);

    private static bool IsError(HttpContext ctx, Exception? ex) =>
        ex is not null || ctx.Response.StatusCode > 499;

    private static bool IsTraceEndpoint(HttpContext ctx, string[] traceEndpoints)
    {
        var endpoint = ctx.GetEndpoint();
        if (endpoint is not null)
        {
            for (int i = 0; i < traceEndpoints.Length; i++)
            {
                if (string.Equals(traceEndpoints[i], endpoint.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }
}