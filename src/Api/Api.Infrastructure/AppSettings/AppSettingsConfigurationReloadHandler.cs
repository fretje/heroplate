using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Domain.Abstractions.Events;
using Heroplate.Api.Domain.AppSettings;

namespace Heroplate.Api.Infrastructure.AppSettings;

public class AppSettingsConfigurationReloadHandler
    : IEventNotificationHandler<EntityCreatedEvent<AppSetting>>,
      IEventNotificationHandler<EntityUpdatedEvent<AppSetting>>,
      IEventNotificationHandler<EntityDeletedEvent<AppSetting>>
{
    private readonly IAppSettingsConfigurationReloader _appSettingsConfig;
    public AppSettingsConfigurationReloadHandler(IAppSettingsConfigurationReloader appSettingsConfig) => _appSettingsConfig = appSettingsConfig;

    public Task Handle(EventNotification<EntityCreatedEvent<AppSetting>> notification, CancellationToken ct) =>
        ReloadAsync();
    public Task Handle(EventNotification<EntityUpdatedEvent<AppSetting>> notification, CancellationToken ct) =>
        ReloadAsync();
    public Task Handle(EventNotification<EntityDeletedEvent<AppSetting>> notification, CancellationToken ct) =>
        ReloadAsync();

    private Task ReloadAsync()
    {
        _appSettingsConfig.Reload();
        return Task.CompletedTask;
    }
}