using Microsoft.Extensions.DependencyInjection;

namespace Heroplate.Api.Infrastructure.Persistence.Initialization;

internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] _seeders;

    public CustomSeederRunner(IServiceProvider serviceProvider) =>
        _seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();

    public async Task RunSeedersAsync(CancellationToken ct)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.InitializeAsync(ct);
        }
    }
}