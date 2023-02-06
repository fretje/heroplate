using Finbuckle.MultiTenant;
using Heroplate.Api.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Serilog;

namespace Heroplate.Api.Infrastructure.Common.Logging;

public class SerilogLoggingActionFilter : IActionFilter
{
    private readonly LoggerSettings _loggerSettings;
    private readonly IDiagnosticContext _diagnosticContext;

    public SerilogLoggingActionFilter(IOptions<LoggerSettings> loggerSettings, IDiagnosticContext diagnosticContext) =>
        (_loggerSettings, _diagnosticContext) = (loggerSettings.Value, diagnosticContext);

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (_loggerSettings.EnableRequestLogging)
        {
            _diagnosticContext.Set("RouteData", context.ActionDescriptor.RouteValues);
            _diagnosticContext.Set("ActionName", context.ActionDescriptor.DisplayName);
            _diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);
            _diagnosticContext.Set("ValidationState", context.ModelState.IsValid);
            if (context.HttpContext.GetMultiTenantContext<HeroTenantInfo>()?.TenantInfo?.Identifier is { } tenant)
            {
                _diagnosticContext.Set("Tenant", tenant);
            }
        }
    }

    // Required by the interface
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}