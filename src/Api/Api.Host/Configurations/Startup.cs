namespace Heroplate.Api.Host.Configurations;

internal static class Startup
{
    internal static IConfigurationBuilder AddConfigurations(this IConfigurationBuilder config, string environmentName)
    {
        const string configurationsDirectory = "Configurations";
        return config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/logger.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/logger.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/hangfire.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/hangfire.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/cache.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/cache.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/cors.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/cors.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/database.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/database.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/mail.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/mail.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/security.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/security.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/openapi.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/openapi.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/signalr.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/signalr.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/localization.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/localization.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}