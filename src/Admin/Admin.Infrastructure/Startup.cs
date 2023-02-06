using System.Globalization;
using Blazored.LocalStorage;
using Fluxor.Persist.Storage;
using Heroplate.Admin.Infrastructure.Auth;
using Heroplate.Admin.Infrastructure.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace Heroplate.Admin.Infrastructure;

public static class Startup
{
    private const string ClientName = "Heroplate.Api";

    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration config) =>
        services
            .AddLocalization(options => options.ResourcesPath = "Resources")
            .AddBlazoredLocalStorage()
            .AddScoped<IStringStateStorage, LocalStateStorage>()
            .AddScoped<IStoreHandler, JsonStoreHandler>()
            .AddMudServices(configuration =>
                {
                    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
                    configuration.SnackbarConfiguration.HideTransitionDuration = 100;
                    configuration.SnackbarConfiguration.ShowTransitionDuration = 100;
                    configuration.SnackbarConfiguration.VisibleStateDuration = 6000;
                    configuration.SnackbarConfiguration.ShowCloseIcon = false;
                })
            .AutoRegisterInterfaces<IAppService>()
            .AutoRegisterInterfaces<IApiService>()
            .AddNotifications()
            .AddAuthentication(config)
            .AddAuthorization()

            // Add Api Http Client.
            .AddHttpClient(ClientName, client =>
                {
                    client.DefaultRequestHeaders.AcceptLanguage.Clear();
                    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(CultureInfo.DefaultThreadCurrentCulture?.TwoLetterISOLanguageName);
                    client.BaseAddress = new Uri(config[ConfigNames.ApiBaseUrl] ?? throw new InvalidOperationException("No ApiBaseUrl defined in app settings."));
                })
                .AddAuthorizationHandler(config)
                .Services
            .AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(ClientName));

    public static IServiceCollection AutoRegisterInterfaces<T>(this IServiceCollection services)
    {
        var @interface = typeof(T);

        var types = @interface
            .Assembly
            .GetExportedTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new
            {
                Service = t.GetInterface($"I{t.Name}"),
                Implementation = t
            })
            .Where(t => t.Service != null);

        foreach (var type in types)
        {
            if (@interface.IsAssignableFrom(type.Service))
            {
                services.AddTransient(type.Service, type.Implementation);
            }
        }

        return services;
    }
}