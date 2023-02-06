using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Heroplate.Api.Contracts.Notifications;
using Microsoft.AspNetCore.SignalR.Client;

namespace Heroplate.Admin.Application.Common.Notifications;

public sealed partial class NotificationConnection : IDisposable, IAsyncDisposable
{
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;

    private readonly CancellationTokenSource _cts = new();
    private IDisposable? _subscription;
    private HubConnection? _hubConnection;

    public ConnectionState ConnectionState =>
        _hubConnection?.State switch
        {
            HubConnectionState.Connected => ConnectionState.Connected,
            HubConnectionState.Disconnected => ConnectionState.Disconnected,
            _ => ConnectionState.Connecting
        };

    public string? ConnectionId => _hubConnection?.ConnectionId;

    public IDisposable SubscribeToStream<TNotification>(Action<TNotification> callback)
    {
        _ = _hubConnection ?? throw new InvalidOperationException("HubConnection is not initialized.");

        var subscription = new StreamSubscription();
        _ = InvokeAsync(async () =>
        {
            var channel = await _hubConnection.StreamAsChannelAsync<TNotification>(typeof(TNotification).Name);
            while (await channel.WaitToReadAsync() && !subscription.IsDisposed)
            {
                while (channel.TryRead(out var notification))
                {
                    callback(notification);
                }
            }
        });
        return subscription;
    }

    private sealed class StreamSubscription : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }

    protected override Task OnInitializedAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{Config[ConfigNames.ApiBaseUrl]}notifications", options =>
                options.AccessTokenProvider =
                    () => TokenProvider.GetAccessTokenAsync())
            .WithAutomaticReconnect(new IndefiniteRetryPolicy())
            .Build();

        _hubConnection.Reconnecting += ex =>
            OnConnectionStateChangedAsync(ConnectionState.Connecting, ex?.Message);

        _hubConnection.Reconnected += id =>
            OnConnectionStateChangedAsync(ConnectionState.Connected, id);

        _hubConnection.Closed += async ex =>
        {
            await OnConnectionStateChangedAsync(ConnectionState.Disconnected, ex?.Message);

            // This shouldn't happen with the IndefiniteRetryPolicy configured above,
            // but just in case it does, we wait a bit and restart the connection again.
            await Task.Delay(5000, _cts.Token);
            await ConnectWithRetryAsync(_cts.Token);
        };

        _subscription = _hubConnection.On<string, JsonObject>(NotificationConstants.NotificationFromServer, async (notificationTypeName, notificationJson) =>
        {
            try
            {
                if (Assembly.GetAssembly(typeof(INotificationMessage))!.GetType(notificationTypeName)
                    is { } notificationType
                    && notificationJson.Deserialize(
                        notificationType,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                        is INotificationMessage notification)
                {
                    if (notification is IFluxorAction action)
                    {
                        Dispatcher.Dispatch(action);
                    }
                    else
                    {
                        await Publisher.PublishAsync(notification);
                    }

                    return;
                }

                Logger.LogError("Invalid Notification Received ({Name}).", notificationTypeName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unhandled Exception while processing {Notification}", notificationTypeName);
                throw;
            }
        });

        // launch the signalR connection in the background.
        // see https://www.dotnetcurry.com/aspnet-core/realtime-app-using-blazor-webassembly-signalr-csharp9
        _ = ConnectWithRetryAsync(_cts.Token);

        return base.OnInitializedAsync();
    }

    private async Task ConnectWithRetryAsync(CancellationToken ct)
    {
        _ = _hubConnection ?? throw new InvalidOperationException("HubConnection can't be null.");

        // Keep trying to until we can start or the token is canceled.
        while (true)
        {
            try
            {
                await _hubConnection.StartAsync(ct);
                await OnConnectionStateChangedAsync(ConnectionState.Connected, _hubConnection.ConnectionId);
                return;
            }
            catch when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (HttpRequestException requestException) when (requestException.StatusCode == HttpStatusCode.Unauthorized)
            {
                // This shouldn't happen, but just in case, go to logout.
                await Authenticator.LogoutAsync();
                return;
            }
            catch
            {
                // Try again in a few seconds. TODO: This could be an incremental interval?
                await Task.Delay(5000, ct);
            }
        }
    }

    private Task OnConnectionStateChangedAsync(ConnectionState state, string? message)
    {
        Logger.LogDebug("Notification Connection State Changed: {State} {Message}", state, message);
        return Publisher.PublishAsync(new ConnectionStateChanged(state, message));
    }

    private bool _disposed;
    public void Dispose()
    {
        if (!_disposed)
        {
            _cts.Cancel();
            _cts.Dispose();
            _subscription?.Dispose();
            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            Dispose();
            await _hubConnection.DisposeAsync();
        }
    }
}

internal sealed class IndefiniteRetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext) =>
        retryContext.PreviousRetryCount switch
        {
            0 => TimeSpan.Zero,
            1 => TimeSpan.FromSeconds(2),
            2 => TimeSpan.FromSeconds(5),
            _ => TimeSpan.FromSeconds(10)
        };
}