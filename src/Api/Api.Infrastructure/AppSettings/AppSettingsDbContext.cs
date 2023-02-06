using Heroplate.Api.Domain.AppSettings;
using Heroplate.Api.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Heroplate.Api.Infrastructure.AppSettings;

internal class AppSettingsDbContext : DbContext
{
    public AppSettingsDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<AppSetting> AppSetting => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaNames.Heroplate);
    }
}