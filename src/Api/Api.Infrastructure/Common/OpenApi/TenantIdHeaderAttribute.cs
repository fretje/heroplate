using Heroplate.Api.Contracts.Multitenancy;

namespace Heroplate.Api.Infrastructure.Common.OpenApi;

public sealed class TenantIdHeaderAttribute : SwaggerHeaderAttribute
{
    public TenantIdHeaderAttribute()
        : base(
            MultitenancyConstants.TenantIdName,
            "Input your tenant Id to access this API",
            "",
            true)
    {
    }
}