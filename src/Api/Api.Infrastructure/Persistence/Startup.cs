using Heroplate.Api.Application.Common.Persistence;
using Heroplate.Api.Domain.Abstractions.Entities;
using Heroplate.Api.Infrastructure.Common;
using Heroplate.Api.Infrastructure.Persistence.ConnectionString;
using Heroplate.Api.Infrastructure.Persistence.Context;
using Heroplate.Api.Infrastructure.Persistence.Initialization;
using Heroplate.Api.Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Heroplate.Api.Infrastructure.Persistence;

internal static class Startup
{
    private static readonly ILogger _logger = Log.ForContext(typeof(Startup));

    internal static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddOptions<DatabaseSettings>()
            .BindConfiguration(nameof(DatabaseSettings))
            .PostConfigure(dbSettings => _logger.Information("Current DB Provider: {dbProvider}", dbSettings.DBProvider))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddDbContext<ApplicationDbContext>((p, m) =>
            {
                var databaseSettings = p.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                m.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString);
            })

            .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
            .AddTransient<ApplicationDbInitializer>()
            .AddTransient<ApplicationDbSeeder>()
            .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)
            .AddTransient<CustomSeederRunner>()

            .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
            .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()

            .AddRepositories();
    }

    internal static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider, string connectionString) =>
        dbProvider.ToLowerInvariant() switch
        {
            DbProviderKeys.SqlServer => builder.UseSqlServer(connectionString, e =>
                e.MigrationsAssembly("Heroplate.Api.Infrastructure.Migrator")),
            _ => throw new InvalidOperationException($"DB Provider {dbProvider} is not supported."),
        };

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Add Repositories
        services.AddScoped(typeof(IRepository<>), typeof(ApplicationDbRepository<>));

        // Search all types that inherit IAggregateRoot
        foreach (var aggregateRootType in
            typeof(IAggregateRoot).Assembly.GetExportedTypes().Union(// domain assembly
            typeof(IRepository<>).Assembly.GetExportedTypes()) // application assembly
                .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && t.IsClass)
                .ToList())
        {
            // Add ReadRepositories.
            services.AddScoped(typeof(IReadRepository<>).MakeGenericType(aggregateRootType), sp =>
                sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)));

            // Decorate the repositories with EventAddingRepositoryDecorators and expose them as IRepositoryWithEvents.
            services.AddScoped(typeof(IRepositoryWithEvents<>).MakeGenericType(aggregateRootType), sp =>
                Activator.CreateInstance(
                    typeof(EventAddingRepositoryDecorator<>).MakeGenericType(aggregateRootType),
                    sp.GetRequiredService(typeof(IRepository<>).MakeGenericType(aggregateRootType)))
                ?? throw new InvalidOperationException($"Couldn't create EventAddingRepositoryDecorator for aggregateRootType {aggregateRootType.Name}"));
        }

        return services;
    }
}