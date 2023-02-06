using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Hangfire;
using Hangfire.Server;
using MediatR;

namespace Heroplate.Api.Infrastructure.BackgroundJobs;

/// <summary>
/// <para>
/// This is the bridge between hangfire and mediator. This works together with HangfireService
/// and enables Enqueueing or Scheduling any IRequest with hangfire.
/// </para>
/// <para>
/// ICommand's get special treatment. They get intercepted and routed through ICommandRunner in
/// stead which takes care of logging the command in the database, sending events and cancellation.
/// </para>
/// </summary>
internal class HangfireMediatorBridge
{
    private readonly ISender _mediator;
    private readonly PerformContext _performContext;
    public HangfireMediatorBridge(ISender mediator, PerformContext performContext) =>
        (_mediator, _performContext) = (mediator, performContext);

    /// <summary>
    /// This is used for enqueing jobs (used from HangfireService.EnqueueAsync.
    /// In this case the tenantId and userId are set via the HeroJobFilter
    /// (when creating the job) and then later picked up in HeroJobActivator.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    [DisplayName("{0}")]
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "jobName is used by Hangfire through the DisplayName attribute")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "jobName is used by Hangfire through the DisplayName attribute")]
    public Task SendAsync(string jobName, IRequest request, CancellationToken hangfireToken)
    {
        return _mediator.Send(request, hangfireToken);
    }

    /// <summary>
    /// This is used for scheduling recurring jobs (used from HangfireService.AddOrUpdate)
    /// we don't get a chance to add parameters using the HeroJobFilter in this case,
    /// so we send tenantId and userId as method arguments.
    /// HeroJobActivator then takes care of getting and setting those values before starting the job.
    /// </summary>
    [AutomaticRetry(Attempts = 0)]
    [DisplayName("{0}")]
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "jobName is used by Hangfire through the DisplayName attribute and tenantId and userId are picked up by the HeroJobActivator")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "jobName is used by Hangfire through the DisplayName attribute and tenantId and userId are picked up by the HeroJobActivator")]
    public async Task SendAsync(string jobName, string tenantId, string userId, IRequest request, TimeSpan? timeout, CancellationToken hangfireToken)
    {
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, hangfireToken);
        if (timeout.HasValue)
        {
            timeoutCts.CancelAfter(timeout.Value);
        }

        await _mediator.Send(request, linkedCts.Token);
    }
}