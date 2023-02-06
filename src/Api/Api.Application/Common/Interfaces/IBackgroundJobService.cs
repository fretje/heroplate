using System.Linq.Expressions;

namespace Heroplate.Api.Application.Common.Interfaces;

public interface IBackgroundJobService : ITransientService
{
    Task<string> EnqueueAsync(IRequest request, CancellationToken ct);
    void AddOrUpdate(string recurringJobId, IRequest request, string cronExpression, TimeSpan? timeout = null, TimeZoneInfo? timeZone = null, string queue = "default");
    void RemoveIfExists(string recurringJobId);

    string Enqueue(Expression<Action> methodCall);
    string Enqueue(Expression<Func<Task>> methodCall);
    string Enqueue<T>(Expression<Action<T>> methodCall);
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    string Schedule(Expression<Action> methodCall, TimeSpan delay);
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);
    string Schedule(Expression<Action> methodCall, DateTimeOffset enqueueAt);
    string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt);
    string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt);
    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);

    bool Delete(string backgroundJobId);
    bool Delete(string backgroundJobId, string fromState);
    bool Requeue(string backgroundJobId);
    bool Requeue(string backgroundJobId, string fromState);
}