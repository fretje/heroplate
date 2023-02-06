using System.Reactive.Subjects;

namespace Heroplate.Api.Infrastructure.Notifications;

internal sealed class NotificationStreamer<TNotification> : IDisposable
{
    private readonly Subject<TNotification> _subject = new();

    public void Send(TNotification notification) => _subject.OnNext(notification);

    public IObservable<TNotification> Receive() => _subject;

    public void Dispose() => _subject.Dispose();
}