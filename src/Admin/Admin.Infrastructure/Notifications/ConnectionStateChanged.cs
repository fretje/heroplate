using Heroplate.Api.Contracts.Notifications;

namespace Heroplate.Admin.Infrastructure.Notifications;

public record ConnectionStateChanged(ConnectionState State, string? Message) : INotificationMessage;