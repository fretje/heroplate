using System.Linq.Expressions;
using System.Text.Json;
using Hangfire;
using Heroplate.Api.Application.Common.Interfaces;
using Heroplate.Api.Infrastructure.Multitenancy;
using MediatR;

namespace Heroplate.Api.Infrastructure.BackgroundJobs;

public class HangfireService : IBackgroundJobService
{
    private readonly HeroTenantInfo _currentTenant;
    private readonly ICurrentUser _currentUser;

    public HangfireService(HeroTenantInfo currentTenant, ICurrentUser currentUser) =>
        (_currentTenant, _currentUser) = (currentTenant, currentUser);

    public Task<string> EnqueueAsync(IRequest request, CancellationToken ct) =>
        Task.FromResult(BackgroundJob.Enqueue<HangfireMediatorBridge>(bridge => bridge.SendAsync(GetDisplayName(request), request, default)));

    public void AddOrUpdate(string recurringJobId, IRequest request, string cronExpression, TimeSpan? timeout = null, TimeZoneInfo? timeZone = null, string queue = "default") =>
        RecurringJob.AddOrUpdate<HangfireMediatorBridge>(
            recurringJobId,
            queue,
            bridge => bridge.SendAsync(GetDisplayName(request), _currentTenant.Id, _currentUser.GetUserId().ToString(), request, timeout, default),
            cronExpression,
            new() { TimeZone = timeZone });

    public void RemoveIfExists(string recurringJobId) =>
        RecurringJob.RemoveIfExists(recurringJobId);

    private static string GetDisplayName(IRequest request) => $"{request.GetType().Name} {JsonSerializer.Serialize(request, request.GetType())}";
    public bool Delete(string backgroundJobId) =>
        BackgroundJob.Delete(backgroundJobId);

    public bool Delete(string backgroundJobId, string fromState) =>
        BackgroundJob.Delete(backgroundJobId, fromState);

    public string Enqueue(Expression<Func<Task>> methodCall) =>
        BackgroundJob.Enqueue(methodCall);

    public string Enqueue<T>(Expression<Action<T>> methodCall) =>
        BackgroundJob.Enqueue(methodCall);

    public string Enqueue(Expression<Action> methodCall) =>
        BackgroundJob.Enqueue(methodCall);

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) =>
        BackgroundJob.Enqueue(methodCall);

    public bool Requeue(string backgroundJobId) =>
        BackgroundJob.Requeue(backgroundJobId);

    public bool Requeue(string backgroundJobId, string fromState) =>
        BackgroundJob.Requeue(backgroundJobId, fromState);

    public string Schedule(Expression<Action> methodCall, TimeSpan delay) =>
        BackgroundJob.Schedule(methodCall, delay);

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay) =>
        BackgroundJob.Schedule(methodCall, delay);

    public string Schedule(Expression<Action> methodCall, DateTimeOffset enqueueAt) =>
        BackgroundJob.Schedule(methodCall, enqueueAt);

    public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt) =>
        BackgroundJob.Schedule(methodCall, enqueueAt);

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay) =>
        BackgroundJob.Schedule(methodCall, delay);

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) =>
        BackgroundJob.Schedule(methodCall, delay);

    public string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt) =>
        BackgroundJob.Schedule(methodCall, enqueueAt);

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt) =>
        BackgroundJob.Schedule(methodCall, enqueueAt);
}