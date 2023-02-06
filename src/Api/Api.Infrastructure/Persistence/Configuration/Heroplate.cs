using Finbuckle.MultiTenant.EntityFrameworkCore;
using Heroplate.Api.Domain.AppSettings;
using Heroplate.Api.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heroplate.Api.Infrastructure.Persistence.Configuration;

public class AppSettingConfig : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder) =>
        builder.IsMultiTenant();
}

public class BrandConfig : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.IsMultiTenant();
        builder.Property(h => h.Name).HasMaxLength(256);
    }
}

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.IsMultiTenant();
        builder.Property(b => b.Name).HasMaxLength(1024);
        builder.Property(p => p.ImagePath).HasMaxLength(2048);
    }
}