using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Heroplate.Api.Infrastructure.AppSettings;

internal class AppSettingsConfigurationSource : IConfigurationSource
{
    public Action<DbContextOptionsBuilder> OptionsAction { get; }
    public AppSettingsConfigurationReloader? Reloader { get; }

    public AppSettingsConfigurationSource(Action<DbContextOptionsBuilder> optionsAction, AppSettingsConfigurationReloader? reloader) =>
        (OptionsAction, Reloader) = (optionsAction, reloader);

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new AppSettingsConfigurationProvider(this);
}