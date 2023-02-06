namespace Heroplate.Api.Domain.MultiTenancy;

public abstract class TenantEvent : DomainEvent
{
    public string TenantId { get; set; } = default!;
    protected TenantEvent(string tenantId) => TenantId = tenantId;
}

public class TenantCreatedEvent : TenantEvent
{
    public TenantCreatedEvent(string tenantId)
        : base(tenantId)
    {
    }
}

public class TenantUpdatedEvent : TenantEvent
{
    public string? Issuer { get; set; }
    public TenantUpdatedEvent(string tenantId, string? issuer)
        : base(tenantId)
        => Issuer = issuer;
}