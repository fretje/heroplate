using System.Globalization;
using Serilog;

namespace Heroplate.Api.Infrastructure.Common.Logging;

public static class StaticLogger
{
    public static void EnsureInitialized()
    {
        if (Log.Logger is not Serilog.Core.Logger)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(formatProvider: CultureInfo.CurrentCulture)
                .CreateLogger();
        }
    }
}