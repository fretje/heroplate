using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Heroplate.Api.Infrastructure.Common;

internal static class ResilientHttpClient
{
    private const string Name = nameof(ResilientHttpClient);

    internal static IServiceCollection AddResilientHttpClient(this IServiceCollection services) =>
        services
            .AddHttpClient(Name)
                .AddTransientHttpErrorPolicy(policy =>
                    policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .Services;

    internal static HttpClient CreateResilientClient(this IHttpClientFactory httpClientFactory) =>
        httpClientFactory.CreateClient(Name);
}