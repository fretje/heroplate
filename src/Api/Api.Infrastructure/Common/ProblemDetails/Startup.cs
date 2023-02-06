using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;

namespace Heroplate.Api.Infrastructure.Common.ProblemDetails;

internal static class Startup
{
    public static IServiceCollection AddHeroProblemDetails(this IServiceCollection services) =>
        services
            .AddProblemDetails(options =>
            {
                options.ValidationProblemStatusCode = 400;
                options.MapFluentValidationException();
                options.Map<Exception>(ex => HeroProblemDetails.Create(ex));
            });
}