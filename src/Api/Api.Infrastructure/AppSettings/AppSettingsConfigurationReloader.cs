using Microsoft.Extensions.Primitives;

namespace Heroplate.Api.Infrastructure.AppSettings;

internal sealed class AppSettingsConfigurationReloader : IAppSettingsConfigurationReloader, IDisposable
{
    private CancellationTokenSource? _changeTokenCts;

    public IChangeToken GetReloadToken()
    {
        _changeTokenCts?.Dispose();

        _changeTokenCts = new CancellationTokenSource();

        return new CancellationChangeToken(_changeTokenCts.Token);
    }

    public void Reload() => _changeTokenCts?.Cancel();

    public void Dispose() => _changeTokenCts?.Dispose();
}