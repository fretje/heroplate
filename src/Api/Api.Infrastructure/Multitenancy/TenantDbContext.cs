using Finbuckle.MultiTenant.Stores;
using Heroplate.Api.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Heroplate.Api.Infrastructure.Multitenancy;

public class TenantDbContext : EFCoreStoreDbContext<HeroTenantInfo>
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HeroTenantInfo>().ToTable("Tenants", SchemaNames.MultiTenancy);
    }
}