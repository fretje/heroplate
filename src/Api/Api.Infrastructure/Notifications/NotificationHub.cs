using Finbuckle.MultiTenant;
using Heroplate.Api.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Heroplate.Api.Infrastructure.Notifications;

[Authorize]
internal class NotificationHub : Hub, ITransientService
{
    private readonly ITenantInfo? _currentTenant;
    private readonly ILogger<NotificationHub> _logger;
    public NotificationHub(ITenantInfo? currentTenant, ILogger<NotificationHub> logger) =>
        (_currentTenant, _logger) = (currentTenant, logger);

    public override async Task OnConnectedAsync()
    {
        if (_currentTenant is null)
        {
            _logger.LogWarning("A client with connectionId {ConnectionId} tried to connect without a current tenant set. Aborting the connection.", Context.ConnectionId);
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"GroupTenant-{_currentTenant.Id}");

        await base.OnConnectedAsync();

        _logger.LogInformation("A client connected to NotificationHub: {ConnectionId}", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"GroupTenant-{_currentTenant!.Id}");

        await base.OnDisconnectedAsync(exception);

        _logger.LogInformation("A client disconnected from NotificationHub: {ConnectionId}", Context.ConnectionId);
    }
}