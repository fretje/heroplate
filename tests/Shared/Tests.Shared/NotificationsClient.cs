using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Heroplate.Api.Contracts.Notifications;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;

namespace Tests.Shared;

public sealed class NotificationsClient : IAsyncDisposable
{
    private readonly HubConnection? _hubConnection;

    private NotificationsClient(HubConnection? hubConnection) => _hubConnection = hubConnection;

    public List<INotificationMessage> Received { get; } = new();

    public static Task<NotificationsClient> StartAsync(TestServer testServer, object? token) =>
        StartAsync(new("http://localhost/"), token, testServer);

    public static async Task<NotificationsClient> StartAsync(Uri serverUrl, object? token, TestServer? testServer = null)
    {
        string accessToken = JsonConvert.SerializeObject(token);
        var hubConnection = new HubConnectionBuilder()
            .WithUrl($"{serverUrl}notifications?access_token={accessToken}", options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.AccessTokenProvider = () => Task.FromResult((string?)accessToken);
                if (testServer is not null)
                {
                    options.SkipNegotiation = true;
                    options.HttpMessageHandlerFactory = _ => testServer.CreateHandler();
                    options.WebSocketFactory = async (context, ct) =>
                    {
                        var wsClient = testServer.CreateWebSocketClient();
                        return await wsClient.ConnectAsync(context.Uri, ct);
                    };
                }
            })
            .Build();

        var connection = new NotificationsClient(hubConnection);
        hubConnection.On<string, JsonObject>(NotificationConstants.NotificationFromServer, (notificationTypeName, notificationJson) =>
        {
            if (Assembly.GetAssembly(typeof(INotificationMessage))!.GetType(notificationTypeName)
                is { } notificationType
                && notificationJson.Deserialize(
                    notificationType,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                    is INotificationMessage notification)
            {
                connection.Received.Add(notification);
            }
        });

        await hubConnection.StartAsync();

        return connection;
    }

    public async Task<TMessage> WaitForAsync<TMessage>(Func<TMessage, bool> predicate)
    {
        TMessage? message = default;
        while (message is null)
        {
            message = Received.OfType<TMessage>().FirstOrDefault(predicate);
            await Task.Delay(100);
        }

        return message;
    }

    public ValueTask DisposeAsync() => _hubConnection is null ? default : _hubConnection.DisposeAsync();
}