using System.Data;
using Finbuckle.MultiTenant;
using Heroplate.Api.Application.Common.Events;
using Heroplate.Api.Application.Common.Interfaces;
using Heroplate.Api.Domain.Abstractions.Entities;
using Heroplate.Api.Infrastructure.Common.Auditing;
using Heroplate.Api.Infrastructure.Identity;
using Heroplate.Api.Infrastructure.Persistence.JsonValueConverter;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Heroplate.Api.Infrastructure.Persistence.Context;

public abstract class BaseDbContext : MultiTenantIdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, ApplicationRoleClaim, IdentityUserToken<string>>
{
    private readonly ICurrentUser _currentUser;
    private readonly ISerializerService _serializer;
    private readonly DatabaseSettings _dbSettings;
    private readonly IEventPublisher _events;
    protected BaseDbContext(ITenantInfo currentTenant, DbContextOptions options, ICurrentUser currentUser, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events)
        : base(currentTenant, options)
    {
        _currentUser = currentUser;
        _serializer = serializer;
        _dbSettings = dbSettings.Value;
        _events = events;
    }

    // Used by Dapper
    public IDbConnection Connection => Database.GetDbConnection();

    public DbSet<Trail> AuditTrails => Set<Trail>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        builder.AppendGlobalQueryFilter<ISoftDelete>(s => s.DeletedOn == null);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
#pragma warning disable IDE0200 // Remove unnecessary lambda expression
        optionsBuilder
            .EnableSensitiveDataLogging();

            // .LogTo(m => System.Diagnostics.Debug.WriteLine(m), Microsoft.Extensions.Logging.LogLevel.Information);
#pragma warning restore IDE0200 // Remove unnecessary lambda expression

        // logging to the console instead
        // .LogTo(Console.WriteLine, LogLevel.Information);
#endif

        if (!string.IsNullOrWhiteSpace(TenantInfo?.ConnectionString))
        {
            optionsBuilder.UseDatabase(_dbSettings.DBProvider, TenantInfo.ConnectionString);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        // Deleted entities are gone after calling base.SaveChanges, so we have to cache the entities with events first.
        var entitiesWithDomainEvents = GetEntitiesWithDomainEvents();

        var auditEntries = HandleAuditingBeforeSaveChanges(_currentUser.GetUserId());

        int result = await base.SaveChangesAsync(ct);

        await HandleAuditingAfterSaveChangesAsync(auditEntries, ct);

        await SendDomainEventsAsync(entitiesWithDomainEvents, ct);

        return result;
    }

    private List<AuditTrail> HandleAuditingBeforeSaveChanges(Guid userId)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.LastModifiedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedOn = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = userId;
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ISoftDelete softDelete)
                    {
                        entry.State = EntityState.Unchanged;
                        foreach (var reference in entry.References.Where(
                            r => r.TargetEntry?.Metadata.IsOwned() is true
                              && r.TargetEntry?.State == EntityState.Deleted))
                        {
                            reference.TargetEntry!.State = EntityState.Unchanged;
                        }

                        softDelete.DeletedBy = userId;
                        softDelete.DeletedOn = DateTime.UtcNow;
                    }

                    break;
            }
        }

        ChangeTracker.DetectChanges();

        var trailEntries = new List<AuditTrail>();
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
            .ToList())
        {
            var trailEntry = new AuditTrail(entry, _serializer)
            {
                TableName = entry.Entity.GetType().Name,
                UserId = userId
            };
            trailEntries.Add(trailEntry);
            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    trailEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    trailEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        trailEntry.TrailType = TrailType.Create;
                        trailEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        trailEntry.TrailType = TrailType.Delete;
                        trailEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (!property.IsModified)
                        {
                            continue;
                        }

                        if (entry.Entity is ISoftDelete
                            && typeof(ISoftDelete).GetProperties().Any(prop => propertyName == prop.Name)
                            && property.OriginalValue == null
                            && property.CurrentValue != null)
                        {
                            trailEntry.ChangedColumns.Add(propertyName);
                            trailEntry.TrailType = TrailType.Delete;
                            trailEntry.OldValues[propertyName] = property.OriginalValue;
                            trailEntry.NewValues[propertyName] = property.CurrentValue;
                            continue;
                        }

                        var typeMapping = property.Metadata.GetTypeMapping();
                        if (typeMapping.Comparer.Equals(property.OriginalValue, property.CurrentValue)
                            || (typeMapping.Converter is { } converter
                                && converter.GetType() is { } converterType && converterType.IsGenericType
                                && converterType.GetGenericTypeDefinition() == typeof(JsonValueConverter<>)
                                && (string?)converter.ConvertToProvider(property.OriginalValue) ==
                                    (string?)converter.ConvertToProvider(property.CurrentValue)))
                        {
                            continue;
                        }

                        trailEntry.ChangedColumns.Add(propertyName);
                        trailEntry.TrailType = TrailType.Update;
                        trailEntry.OldValues[propertyName] = property.OriginalValue;
                        trailEntry.NewValues[propertyName] = property.CurrentValue;

                        break;
                }
            }
        }

        foreach (var auditEntry in trailEntries.Where(e => !e.HasTemporaryProperties))
        {
            AuditTrails.Add(auditEntry.ToAuditTrail());
        }

        return trailEntries.Where(e => e.HasTemporaryProperties).ToList();
    }

    private Task HandleAuditingAfterSaveChangesAsync(List<AuditTrail> trailEntries, CancellationToken ct = default)
    {
        if (trailEntries == null || trailEntries.Count == 0)
        {
            return Task.CompletedTask;
        }

        foreach (var entry in trailEntries)
        {
            foreach (var prop in entry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            AuditTrails.Add(entry.ToAuditTrail());
        }

        return SaveChangesAsync(ct);
    }

    private IEntity[] GetEntitiesWithDomainEvents() =>
        ChangeTracker.Entries<IEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToArray();

    private async Task SendDomainEventsAsync(IEntity[] entitiesWithDomainEvents, CancellationToken ct)
    {
        foreach (var entity in entitiesWithDomainEvents)
        {
            var domainEvents = entity.DomainEvents.ToArray();
            entity.DomainEvents.Clear();
            foreach (var domainEvent in domainEvents)
            {
                await _events.PublishAsync(domainEvent, ct);
            }
        }
    }
}