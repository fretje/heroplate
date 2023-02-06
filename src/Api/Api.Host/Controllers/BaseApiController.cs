using Heroplate.Api.Application.Common.Interfaces;
using MediatR;

namespace Heroplate.Api.Host.Controllers;

[ApiController]
public class BaseApiController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    private IBackgroundJobService? _backgroundJobs;
    protected IBackgroundJobService BackgroundJobs =>
        _backgroundJobs ??= HttpContext.RequestServices.GetRequiredService<IBackgroundJobService>();
}